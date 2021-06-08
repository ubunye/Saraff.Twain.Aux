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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Saraff.Twain.Aux
{
    public sealed class TwainExternalProcess
    {
        public static void Execute(string fileName, Action<Twain32> execCallback)
        {
            Execute(fileName, (twain, host) => execCallback(twain));
        }

        public static void Execute(string fileName, Action<Twain32, IDisposable> execCallback)
        {
            using (var proc = AuxProcess.CreateProcess(fileName))
            {
                proc.Begin();

                execCallback(new Twain32RealProxy(proc).GetTransparentProxy() as Twain32, proc);

                proc.End();
            }
        }

        public static void Handler(Twain32 twain32)
        {
            try
            {
                #region Method Handler

                Action _method = () =>
                {
                    var result = TwainProxy.Execute(twain32, Convert.FromBase64String(Console.In.ReadLine() ?? string.Empty));

                    Console.Out.WriteLine(Separators.BeginResponse);
                    Console.Out.WriteLine(Convert.ToBase64String(result));
                    Console.Out.WriteLine(Separators.EndResponse);
                };

                #endregion

                #region Event Handler

                Action<EventHandlerTwainCommand> _event = command =>
                {
                    Console.Out.WriteLine(Separators.BeginEvent);
                    try
                    {
                        Console.Out.WriteLine(Convert.ToBase64String(command.ToArray()));
                        for (string response; !string.IsNullOrEmpty(response = Console.In.ReadLine());)
                        {
                            if (response == Separators.BeginRequest)
                            {
                                _method();
                                continue;
                            }

                            var res = ((EventHandlerTwainCommand)TwainCommand.FromArray(
                                Convert.FromBase64String(response))).Args;
                            foreach (var property in res.GetType().GetProperties())
                            {
                                var setter = property.GetSetMethod();
                                if (setter != null)
                                    setter.Invoke(command.Args, new[] { property.GetValue(res, null) });
                            }

                            break;
                        }
                    }
                    finally
                    {
                        Console.Out.WriteLine(Separators.EndEvent);
                    }
                };

                #endregion

                #region AcquireError

                twain32.AcquireError += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.AcquireError),
                        Args = e
                    });
                };

                #endregion

                #region XferDone

                twain32.XferDone += (sender, e) =>
                {
                    var args = new Twain32.SerializableCancelEventArgs { Cancel = e.Cancel };
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.XferDone),
                        Args = args
                    });
                    e.Cancel = args.Cancel;
                };

                #endregion

                #region EndXfer

                twain32.EndXfer += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.EndXfer),
                        Args = e
                    });
                };

                #endregion

                #region SetupMemXferEvent

                twain32.SetupMemXferEvent += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.SetupMemXferEvent),
                        Args = e
                    });
                };

                #endregion

                #region MemXferEvent

                twain32.MemXferEvent += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.MemXferEvent),
                        Args = e
                    });
                };

                #endregion

                #region SetupFileXferEvent

                twain32.SetupFileXferEvent += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.SetupFileXferEvent),
                        Args = e
                    });
                };

                #endregion

                #region FileXferEvent

                twain32.FileXferEvent += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.FileXferEvent),
                        Args = e
                    });
                };

                #endregion

                #region TwainStateChanged

                twain32.TwainStateChanged += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.TwainStateChanged),
                        Args = e
                    });
                };

                #endregion

                #region AcquireCompleted

                twain32.AcquireCompleted += (sender, e) =>
                {
                    _event(new EventHandlerTwainCommand
                    {
                        Member = typeof(Twain32).GetEvent(Twain32Events.AcquireCompleted),
                        Args = e
                    });
                };

                #endregion

                Console.Out.WriteLine(Separators.Ready);
                for (string query;
                    !string.IsNullOrEmpty(query = Console.In.ReadLine()) && query != Separators.End;)
                    if (query == Separators.BeginRequest)
                        _method();
            }
            catch (Exception ex)
            {
                try
                {
                    Console.Error.WriteLine(Separators.BeginException);
                    for (var exception = ex; exception != null; exception = exception.InnerException)
                    {
                        Console.Error.WriteLine("{0}: {1}", exception.GetType().Name, exception.Message);
                        Console.Error.WriteLine(exception.StackTrace);
                        Console.Error.WriteLine();
                    }

                    Console.Error.WriteLine(Separators.EndException);
                }
                catch
                {
                    // ignored
                }
            }
        }

        internal sealed class AuxProcess : IDisposable
        {
            private Process _proc;

            public bool IsValid { get; private set; }

            #region IDisposable

            public void Dispose()
            {
                if (_proc == null)
                    return;

                try
                {
                    _proc.Kill();
                }
                catch
                {
                    // ignored
                }

                _proc.StandardInput.Dispose();
                _proc.StandardOutput.Dispose();
                _proc.StandardError.Dispose();
                _proc.Dispose();
            }

            #endregion

            public static AuxProcess CreateProcess(string fileName)
            {
                return new AuxProcess
                {
                    _proc = Environment.UserInteractive
                        ? Process.Start(new ProcessStartInfo
                        {
                            CreateNoWindow = true,
                            WorkingDirectory = Directory.GetCurrentDirectory(),
                            FileName = fileName,
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            StandardOutputEncoding = Encoding.GetEncoding(866),
                            RedirectStandardError = true,
                            StandardErrorEncoding = Encoding.GetEncoding(866)
                        })
                        : ImpersonationUtils.RunAsCurrentUser(fileName)
                };
            }

            public void Begin()
            {
                for (string response;
                    (IsValid = !string.IsNullOrEmpty(response = _proc.StandardOutput.ReadLine())) &&
                    response != Separators.Ready;)
                {
                }
            }

            public void End()
            {
                if (IsValid) _proc.StandardInput.WriteLine(Separators.End);
            }

            public object Execute(TwainCommand command)
            {
                if (!IsValid)
                    throw GetException() ?? new InvalidOperationException();

                _proc.StandardInput.WriteLine(Separators.BeginRequest);
                _proc.StandardInput.WriteLine(Convert.ToBase64String(command.ToArray()));
                for (string response;
                    (IsValid = !string.IsNullOrEmpty(response = _proc.StandardOutput.ReadLine())) &&
                    response != Separators.BeginResponse;)
                    if (response == Separators.BeginEvent)
                    {
                        _OnEvent();
                    }

                for (string data;
                    IsValid && (IsValid = !string.IsNullOrEmpty(data = _proc.StandardOutput.ReadLine()));)
                    try
                    {
                        return TwainCommand.FromArray(Convert.FromBase64String(data)).Result;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                    finally
                    {
                        for (string response;
                            IsValid &&
                            (IsValid = !string.IsNullOrEmpty(response = _proc.StandardOutput.ReadLine())) &&
                            response != Separators.EndResponse;)
                        {
                        }
                    }

                return GetException() ?? new InvalidOperationException();

            }

            private Exception GetException()
            {
                for (string response; !string.IsNullOrEmpty(response = _proc.StandardError.ReadLine());)
                    if (response == Separators.BeginException)
                    {
                        var message = string.Empty;
                        for (string response2;
                            !string.IsNullOrEmpty(response2 = _proc.StandardError.ReadLine()) &&
                            response2 != Separators.EndException;)
                            message += $"{response2}{Environment.NewLine}";
                        return new InvalidOperationException(message);
                    }

                return null;
            }

            private void _OnEvent()
            {
                for (string eventString;
                    IsValid && (IsValid = !string.IsNullOrEmpty(eventString = _proc.StandardOutput.ReadLine()));)
                {
                    var res = TwainCommand.FromArray(Convert.FromBase64String(eventString)) as EventHandlerTwainCommand;

                    if (res != null)
                        FireEvent?.Invoke(res);

                    if (res != null)
                        _proc.StandardInput.WriteLine(Convert.ToBase64String(res.ToArray()));

                    break;
                }

                for (string response;
                    IsValid && (IsValid = !string.IsNullOrEmpty(response = _proc.StandardOutput.ReadLine())) &&
                    response != Separators.EndEvent;)
                {
                }
            }

            public event Action<EventHandlerTwainCommand> FireEvent;
        }

        private static class Separators
        {
            public const string Ready = "{DA63824B-9931-4363-A606-6160A2211979}";
            public const string BeginRequest = "{ECE0AE56-349A-4318-92D6-F7848362417B}";
            public const string BeginResponse = "{7B261FEB-2DA3-43D7-B64C-7ACFD6F931BF}";
            public const string EndResponse = "{315748BD-D336-4C1D-AADE-E5EC6D3B11B7}";
            public const string End = "{8ACDCA33-EF39-4CEE-B337-CC6CE832832A}";
            public const string BeginException = "{78CD0BB5-6F2B-44BC-B16D-60D564386400}";
            public const string EndException = "{0FD9554B-6AC7-4A45-8BB0-52C5587DAA21}";
            public const string BeginEvent = "{496D8295-FCAD-4236-AD7D-3BEEA7489FB3}";
            public const string EndEvent = "{A1EF8BC7-9016-4FF1-962A-8F2C9726342C}";
        }
    }
}