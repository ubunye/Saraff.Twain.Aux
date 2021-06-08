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
using System.Collections.Generic;
using System.Reflection;

namespace Saraff.Twain.Aux
{
    internal sealed class TwainProxy
    {
        private Twain32 Twain32 { get; set; }

        private object this[Type type]
        {
            get
            {
                if (type == typeof(Twain32.TwainPalette)) return Twain32.Palette;
                return Twain32;
            }
        }

        public static IEnumerable<byte[]> Execute(Twain32 twain32, IEnumerable<byte[]> commands)
        {
            var proxy = new TwainProxy { Twain32 = twain32 };
            foreach (var data in commands)
                for (var command = TwainCommand.FromArray(data); command != null;)
                {
                    proxy._ExecuteCore(command);
                    yield return command.ToArray();
                    break;
                }
        }

        public static byte[] Execute(Twain32 twain32, byte[] command)
        {
            var cmd = TwainCommand.FromArray(command);
            new TwainProxy { Twain32 = twain32 }._ExecuteCore(cmd);
            return cmd.ToArray();
        }

        private void _ExecuteCore(TwainCommand command)
        {
            try
            {
                for (var method = command as MethodTwainCommand; method != null;)
                {
                    command.Result =
                        ((MethodInfo)method.Member).Invoke(this[method.Member.DeclaringType], method.Parameters);
                    return;
                }
            }
            catch (Exception ex)
            {
                command.Result = ex;
            }
        }
    }
}