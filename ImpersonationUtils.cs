/* Этот файл является частью примеров использования библиотеки Saraff.Twain.NET
 * © SARAFF SOFTWARE (Кирножицкий Андрей), 2011.
 * Saraff.Twain.NET - свободная программа: вы можете перераспространять ее и/или
 * изменять ее на условиях Меньшей Стандартной общественной лицензии GNU в том виде,
 * в каком она была опубликована Фондом свободного программного обеспечения;
 * либо версии 3 лицензии, либо (по вашему выбору) любой более поздней
 * версии.
 * Saraff.Twain.NET распространяется в надежде, что она будет полезной,
 * но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА
 * или ПРИГОДНОСТИ ДЛЯ ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Меньшей Стандартной
 * общественной лицензии GNU.
 * Вы должны были получить копию Меньшей Стандартной общественной лицензии GNU
 * вместе с этой программой. Если это не так, см.
 * <http://www.gnu.org/licenses/>.)
 * 
 * This file is part of samples of Saraff.Twain.NET.
 * © SARAFF SOFTWARE (Kirnazhytski Andrei), 2011.
 * Saraff.Twain.NET is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * Saraff.Twain.NET is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public License
 * along with Saraff.Twain.NET. If not, see <http://www.gnu.org/licenses/>.
 * 
 * PLEASE SEND EMAIL TO:  twain@saraff.ru.
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Saraff.Twain.Aux
{
    internal static class ImpersonationUtils
    {
        public static Process RunAsCurrentUser(string fileName)
        {
            var token = _GetPrimaryToken();
            try
            {
                for (var env = IntPtr.Zero;
                    Win32.CreateEnvironmentBlock(ref env, token, false) && env != IntPtr.Zero;)
                    try
                    {
                        var result = _LaunchProcessAsUser(fileName, token, env, out var hStdIn, out var hStdOut,
                            out var hStdError);

                        var type = typeof(Process);
                        for (var field = type.GetField("standardInput",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            field != null && hStdIn != IntPtr.Zero && hStdIn != new IntPtr(-1);)
                        {
                            field.SetValue(result,
                                new StreamWriter(new FileStream(new SafeFileHandle(hStdIn, true), FileAccess.Write),
                                    Encoding.GetEncoding(866))
                                {
                                    AutoFlush = true
                                });
                            break;
                        }

                        for (var field = type.GetField("standardOutput",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            field != null && hStdOut != IntPtr.Zero && hStdOut != new IntPtr(-1);)
                        {
                            field.SetValue(result,
                                new StreamReader(new FileStream(new SafeFileHandle(hStdOut, true), FileAccess.Read),
                                    Encoding.GetEncoding(866), true));
                            break;
                        }

                        for (var field = type.GetField("standardError",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            field != null && hStdError != IntPtr.Zero && hStdError != new IntPtr(-1);)
                        {
                            field.SetValue(result,
                                new StreamReader(new FileStream(new SafeFileHandle(hStdError, true), FileAccess.Read),
                                    Encoding.GetEncoding(866), true));
                            break;
                        }

                        return result;
                    }
                    finally
                    {
                        Win32.DestroyEnvironmentBlock(env);
                    }

                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreateEnvironmentBlock failed.");
            }
            finally
            {
                Win32.CloseHandle(token);
            }
        }

        public static IDisposable ImpersonateCurrentUser()
        {
            return new WindowsIdentity(_GetPrimaryToken()).Impersonate();
        }

        private static Process _LaunchProcessAsUser(string fileName, IntPtr token, IntPtr envBlock, out IntPtr hStdIn,
            out IntPtr hStdOut, out IntPtr hStdError)
        {
            var saProcess = new SecurityAttributes
            {
                nLength = Marshal.SizeOf(typeof(SecurityAttributes))
            };
            var saThread = new SecurityAttributes
            {
                nLength = Marshal.SizeOf(typeof(SecurityAttributes))
            };
            var pi = new _ProcessInformation();

            var si = new StartupInfo
            {
                cb = Marshal.SizeOf(typeof(StartupInfo)),
                lpDesktop = @"WinSta0\Default",
                wShowWindow = SW.Hide,
                dwFlags = StartFlags.UseShowWindow | StartFlags.UseStdHandles
            };

            _CreatePipe(out si.hStdInput, out hStdIn);
            if (!Win32.SetHandleInformation(si.hStdInput, HandleFlags.Inherit, HandleFlags.Inherit))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SetHandleInformation failed.");

            _CreatePipe(out hStdOut, out si.hStdOutput);
            if (!Win32.SetHandleInformation(si.hStdOutput, HandleFlags.Inherit, HandleFlags.Inherit))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SetHandleInformation failed.");

            _CreatePipe(out hStdError, out si.hStdError);
            if (!Win32.SetHandleInformation(si.hStdError, HandleFlags.Inherit, HandleFlags.Inherit))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SetHandleInformation failed.");

            if (!Win32.CreateProcessAsUser(token, null, fileName, saProcess, saThread, true,
                ProcessCreationFlags.CreateUnicodeEnvironment, envBlock, null, si, pi))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreateProcessAsUser failed.");
            Win32.CloseHandle(pi.hProcess);
            Win32.CloseHandle(pi.hThread);
            Win32.CloseHandle(si.hStdInput);
            Win32.CloseHandle(si.hStdOutput);
            Win32.CloseHandle(si.hStdError);
            return Process.GetProcessById(pi.dwProcessId);
        }

        private static IntPtr _GetPrimaryToken()
        {
            for (var process = Process.GetProcessesByName("explorer").FirstOrDefault(); process != null;)
            {
                var token = IntPtr.Zero;

                if (!Win32.OpenProcessToken(process.Handle, TokenAccessLevels.Duplicate, ref token))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "OpenProcessToken failed.");

                try
                {
                    var sa = new SecurityAttributes
                    {
                        nLength = Marshal.SizeOf(typeof(SecurityAttributes))
                    };

                    var primaryToken = IntPtr.Zero;
                    if (!Win32.DuplicateTokenEx(token, TokenAccessLevels.AllAccess, sa,
                        TokenImpersonationLevel.Impersonation, TokenType.Primary, ref primaryToken))
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "DuplicateTokenEx failed.");
                    return primaryToken;
                }
                finally
                {
                    Win32.CloseHandle(token);
                }
            }

            throw new InvalidOperationException("Could not find explorer.exe.");
        }

        private static void _CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe)
        {
            var saPipe = new SecurityAttributes
            {
                nLength = Marshal.SizeOf(typeof(SecurityAttributes)),
                bInheritHandle = true
            };
            if (!Win32.CreatePipe(out hReadPipe, out hWritePipe, saPipe, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreatePipe failed.");
        }

        private sealed class Win32
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool CreateProcessAsUser(
                IntPtr hToken,
                string lpApplicationName,
                string lpCommandLine,
                [In][Out] SecurityAttributes lpProcessAttributes,
                [In][Out] SecurityAttributes lpThreadAttributes,
                bool bInheritHandles,
                ProcessCreationFlags dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                [In][Out] StartupInfo lpStartupInfo,
                [Out] _ProcessInformation lpProcessInformation);

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool DuplicateTokenEx(IntPtr hExistingToken, TokenAccessLevels dwDesiredAccess,
                [In][Out] SecurityAttributes lpThreadAttributes, TokenImpersonationLevel impersonationLevel,
                TokenType dwTokenType, ref IntPtr phNewToken);

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool OpenProcessToken(IntPtr hProcess, TokenAccessLevels dwDesiredAccess,
                ref IntPtr hToken);

            [DllImport("userenv.dll", SetLastError = true)]
            internal static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

            [DllImport("userenv.dll", SetLastError = true)]
            internal static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll")]
            internal static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
                [In][Out] SecurityAttributes lpPipeAttributes, uint nSize);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool SetHandleInformation(IntPtr handle, HandleFlags dwMask, HandleFlags dwFlags);
        }

        #region Nested

        [StructLayout(LayoutKind.Sequential)]
        private class SecurityAttributes
        {
            public bool bInheritHandle;
            public IntPtr lpSecurityDescriptor;
            public int nLength;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class StartupInfo
        {
            public int cb;

            public short cbReserved2;
            public int dwFillAttribute;

            [MarshalAs(UnmanagedType.U4)] public StartFlags dwFlags;

            public int dwX;
            public int dwXCountChars;
            public int dwXSize;
            public int dwY;
            public int dwYCountChars;
            public int dwYSize;
            public IntPtr hStdError;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;

            [MarshalAs(UnmanagedType.LPStr)] public string lpDesktop;

            [MarshalAs(UnmanagedType.LPStr)] public string lpReserved;

            public IntPtr lpReserved2;

            [MarshalAs(UnmanagedType.LPStr)] public string lpTitle;

            [MarshalAs(UnmanagedType.U2)] public SW wShowWindow;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class _ProcessInformation
        {
            public int dwProcessId;
            public int dwThreadId;
            public IntPtr hProcess;
            public IntPtr hThread;
        }

        private enum TokenType
        {
            Primary = 1,
            Impersonation = 2
        }

        [Flags]
        private enum StartFlags : uint
        {
            ForceOnFeedback = 0x00000040,
            ForceOffFeedback = 0x00000080,
            PreventPinning = 0x00002000,
            RunFullSCREEN = 0x00000020,
            TitleIsAppId = 0x00001000,
            TitleIsLinkName = 0x00000800,
            UntrustedSource = 0x00008000,
            UseCountChars = 0x00000008,
            UseFillAttribute = 0x00000010,
            UseHotKey = 0x00000200,
            UsePosition = 0x00000004,
            UseShowWindow = 0x00000001,
            UseSize = 0x00000002,
            UseStdHandles = 0x00000100
        }

        private enum SW : ushort
        {
            ForceMinimize = 11,
            Hide = 0,
            Maximize = 3,
            Minimize = 6,
            Restore = 9,
            Show = 5,
            ShowDefault = 10,
            ShowMaximized = 3,
            ShowMinimized = 2,
            ShowMinNoActive = 7,
            ShowNA = 8,
            ShowNoActivate = 4,
            ShowNormal = 1
        }

        [Flags]
        private enum ProcessCreationFlags
        {
            CreateBreakawayFromJob = 0x01000000,
            CreateDefaultErrorMode = 0x04000000,
            CreateNewConsole = 0x00000010,
            CreateNewProcess_Group = 0x00000200,
            CreateNoWindow = 0x08000000,
            CreateProtectedProcess = 0x00040000,
            CreatePreserveCodeAuthzLevel = 0x02000000,
            CreateSeparateWowVdm = 0x00000800,
            CreateSharedWowVdm = 0x00001000,
            CreateSuspended = 0x00000004,
            CreateUnicodeEnvironment = 0x00000400,
            DebugOnlyThisProcess = 0x00000002,
            DebugProcess = 0x00000001,
            DetachedProcess = 0x00000008,
            ExtendedStartupInfoPresent = 0x00080000,
            InheritParentAffinity = 0x00010000
        }

        [Flags]
        private enum HandleFlags : uint
        {
            None = 0,
            Inherit = 1,
            ProtectFromClose = 2
        }

        #endregion
    }
}