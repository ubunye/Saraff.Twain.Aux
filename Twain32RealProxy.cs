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
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security.Permissions;

namespace Saraff.Twain.Aux
{
    internal sealed class Twain32RealProxy : RealProxy
    {
        private readonly TwainExternalProcess.AuxProcess _aux;
        private TwainCapabilities _capabilities;
        private Twain32.TwainPalette _palette;

        internal Twain32RealProxy(TwainExternalProcess.AuxProcess aux) : base(typeof(Twain32))
        {
            _aux = aux;
            _aux.FireEvent += _FireEvent;
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override IMessage Invoke(IMessage msg)
        {
            var message = msg as IMethodCallMessage;
            try
            {
                var args = message?.Args;

                #region SpecialName

                if (message != null && message.MethodBase.IsSpecialName)
                {
                    var name = message.MethodBase.Name.Split(new[] { "_" }, 2, StringSplitOptions.None);
                    switch (name[0])
                    {
                        case "get":
                            switch (name[1])
                            {
                                case "Capabilities":
                                    return new ReturnMessage(Capabilities, args, 0, message.LogicalCallContext, message);
                                case "Palette":
                                    return new ReturnMessage(Palette, args, 0, message.LogicalCallContext, message);
                            }

                            break;
                        case "add":
                            switch (name[1])
                            {
                                case Twain32Events.AcquireCompleted:
                                    AcquireCompleted += args[0] as EventHandler;
                                    break;
                                case Twain32Events.AcquireError:
                                    AcquireError += args[0] as EventHandler<Twain32.AcquireErrorEventArgs>;
                                    break;
                                case Twain32Events.XferDone:
                                    XferDone += args[0] as EventHandler<Twain32.XferDoneEventArgs>;
                                    break;
                                case Twain32Events.EndXfer:
                                    EndXfer += args[0] as EventHandler<Twain32.EndXferEventArgs>;
                                    break;
                                case Twain32Events.SetupMemXferEvent:
                                    SetupMemXferEvent += args[0] as EventHandler<Twain32.SetupMemXferEventArgs>;
                                    break;
                                case Twain32Events.MemXferEvent:
                                    MemXferEvent += args[0] as EventHandler<Twain32.MemXferEventArgs>;
                                    break;
                                case Twain32Events.SetupFileXferEvent:
                                    SetupFileXferEvent += args[0] as EventHandler<Twain32.SetupFileXferEventArgs>;
                                    break;
                                case Twain32Events.FileXferEvent:
                                    FileXferEvent += args[0] as EventHandler<Twain32.FileXferEventArgs>;
                                    break;
                                case Twain32Events.TwainStateChanged:
                                    TwainStateChanged += args[0] as EventHandler<Twain32.TwainStateEventArgs>;
                                    break;
                                default:
                                    return new ReturnMessage(new NotImplementedException(), message);
                            }

                            return new ReturnMessage(null, args, 0, message.LogicalCallContext, message);
                        case "remove":
                            switch (name[1])
                            {
                                case Twain32Events.AcquireCompleted:
                                    AcquireCompleted -= args[0] as EventHandler;
                                    break;
                                case Twain32Events.AcquireError:
                                    AcquireError -= args[0] as EventHandler<Twain32.AcquireErrorEventArgs>;
                                    break;
                                case Twain32Events.XferDone:
                                    XferDone -= args[0] as EventHandler<Twain32.XferDoneEventArgs>;
                                    break;
                                case Twain32Events.EndXfer:
                                    EndXfer -= args[0] as EventHandler<Twain32.EndXferEventArgs>;
                                    break;
                                case Twain32Events.SetupMemXferEvent:
                                    SetupMemXferEvent -= args[0] as EventHandler<Twain32.SetupMemXferEventArgs>;
                                    break;
                                case Twain32Events.MemXferEvent:
                                    MemXferEvent -= args[0] as EventHandler<Twain32.MemXferEventArgs>;
                                    break;
                                case Twain32Events.SetupFileXferEvent:
                                    SetupFileXferEvent -= args[0] as EventHandler<Twain32.SetupFileXferEventArgs>;
                                    break;
                                case Twain32Events.FileXferEvent:
                                    FileXferEvent -= args[0] as EventHandler<Twain32.FileXferEventArgs>;
                                    break;
                                case Twain32Events.TwainStateChanged:
                                    TwainStateChanged -= args[0] as EventHandler<Twain32.TwainStateEventArgs>;
                                    break;
                                default:
                                    return new ReturnMessage(new NotImplementedException(), message);
                            }

                            return new ReturnMessage(null, args, 0, message.LogicalCallContext, message);
                    }
                }

                #endregion

                var result = _aux.Execute(new MethodTwainCommand
                { Member = message?.MethodBase, Parameters = args });

                for (Exception ex = result as Exception, ex2 = result as TargetInvocationException; ex != null;)
                    return new ReturnMessage(ex2 != null ? ex2.InnerException : ex, message);

                return new ReturnMessage(result, args, 0, message?.LogicalCallContext, message);

            }
            catch (Exception ex)
            {
                return new ReturnMessage(ex, message);
            }
        }

        private void _FireEvent(EventHandlerTwainCommand obj)
        {
            switch (obj.Member.Name)
            {
                case Twain32Events.AcquireCompleted:
                    for (var args = obj.Args; AcquireCompleted != null && args != null;)
                    {
                        AcquireCompleted(this, args);
                        break;
                    }

                    break;
                case Twain32Events.AcquireError:
                    for (var args = obj.Args as Twain32.AcquireErrorEventArgs; AcquireError != null && args != null;)
                    {
                        AcquireError(this, args);
                        break;
                    }

                    break;
                case Twain32Events.XferDone:
                    for (var args = obj.Args as Twain32.SerializableCancelEventArgs;
                        XferDone != null && args != null;)
                    {
                        var info = Delegate.CreateDelegate(
                            typeof(Twain32).GetNestedType("GetImageInfoCallback", BindingFlags.NonPublic),
                            GetTransparentProxy(),
                            typeof(Twain32).GetMethod("_GetImageInfo", BindingFlags.Instance | BindingFlags.NonPublic));
                        var extInfo = Delegate.CreateDelegate(
                            typeof(Twain32).GetNestedType("GetExtImageInfoCallback", BindingFlags.NonPublic),
                            GetTransparentProxy(),
                            typeof(Twain32).GetMethod("_GetExtImageInfo",
                                BindingFlags.Instance | BindingFlags.NonPublic));
                        var args2 = _CreateInstance<Twain32.XferDoneEventArgs>(info, extInfo);
                        XferDone?.Invoke(this, args2);
                        args.Cancel = args2.Cancel;
                        break;
                    }

                    break;
                case Twain32Events.EndXfer:
                    for (var args = obj.Args as Twain32.EndXferEventArgs; EndXfer != null && args != null;)
                    {
                        EndXfer(this, args);
                        break;
                    }

                    break;
                case Twain32Events.SetupMemXferEvent:
                    for (var args = obj.Args as Twain32.SetupMemXferEventArgs;
                        SetupMemXferEvent != null && args != null;)
                    {
                        SetupMemXferEvent(this, args);
                        break;
                    }

                    break;
                case Twain32Events.MemXferEvent:
                    for (var args = obj.Args as Twain32.MemXferEventArgs;
                        MemXferEvent != null && args != null;)
                    {
                        MemXferEvent(this, args);
                        break;
                    }

                    break;
                case Twain32Events.SetupFileXferEvent:
                    for (var args = obj.Args as Twain32.SetupFileXferEventArgs;
                        SetupFileXferEvent != null && args != null;)
                    {
                        SetupFileXferEvent(this, args);
                        break;
                    }

                    break;
                case Twain32Events.FileXferEvent:
                    for (var args = obj.Args as Twain32.FileXferEventArgs;
                        FileXferEvent != null && args != null;)
                    {
                        FileXferEvent(this, args);
                        break;
                    }

                    break;
                case Twain32Events.TwainStateChanged:
                    for (var args = obj.Args as Twain32.TwainStateEventArgs;
                        TwainStateChanged != null && args != null;)
                    {
                        TwainStateChanged(this, args);
                        break;
                    }

                    break;
            }
        }

        private static T _CreateInstance<T>(params object[] args) where T : class
        {
            return Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, args,
                null) as T;
        }

        #region Twain32 Properties

        private TwainCapabilities Capabilities
        {
            get
            {
                if (_capabilities == null) _capabilities = _CreateInstance<TwainCapabilities>(GetTransparentProxy());
                return _capabilities;
            }
        }

        private Twain32.TwainPalette Palette
        {
            get
            {
                if (_palette == null)
                    _palette = new TwainPaletteRealProxy(_aux).GetTransparentProxy() as Twain32.TwainPalette;
                return _palette;
            }
        }

        #endregion

        #region Twain32 Events

        private event EventHandler AcquireCompleted;

        private event EventHandler<Twain32.AcquireErrorEventArgs> AcquireError;

        private event EventHandler<Twain32.XferDoneEventArgs> XferDone;

        private event EventHandler<Twain32.EndXferEventArgs> EndXfer;

        private event EventHandler<Twain32.SetupMemXferEventArgs> SetupMemXferEvent;

        private event EventHandler<Twain32.MemXferEventArgs> MemXferEvent;

        private event EventHandler<Twain32.SetupFileXferEventArgs> SetupFileXferEvent;

        private event EventHandler<Twain32.FileXferEventArgs> FileXferEvent;

        private event EventHandler<Twain32.TwainStateEventArgs> TwainStateChanged;

        #endregion
    }

    internal sealed class Twain32Events
    {
        public const string AcquireCompleted = "AcquireCompleted";
        public const string AcquireError = "AcquireError";
        public const string XferDone = "XferDone";
        public const string EndXfer = "EndXfer";
        public const string SetupMemXferEvent = "SetupMemXferEvent";
        public const string MemXferEvent = "MemXferEvent";
        public const string SetupFileXferEvent = "SetupFileXferEvent";
        public const string FileXferEvent = "FileXferEvent";
        public const string TwainStateChanged = "TwainStateChanged";
        public const string DeviceEvent = "DeviceEvent";
    }
}