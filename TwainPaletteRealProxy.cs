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
    internal sealed class TwainPaletteRealProxy : RealProxy
    {
        private readonly TwainExternalProcess.AuxProcess _aux;

        internal TwainPaletteRealProxy(TwainExternalProcess.AuxProcess aux) : base(typeof(Twain32.TwainPalette))
        {
            _aux = aux;
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override IMessage Invoke(IMessage msg)
        {
            var message = msg as IMethodCallMessage;
            try
            {
                var args = message?.Args;

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
    }
}