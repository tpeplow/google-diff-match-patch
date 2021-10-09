﻿/*
 * Copyright 2008 Google Inc. All Rights Reserved.
 * Author: fraser@google.com (Neil Fraser)
 * Author: anteru@developer.shelter13.net (Matthaeus G. Chajdas)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Diff Match and Patch
 * http://code.google.com/p/google-diff-match-patch/
 */

using System;
using System.Collections.Generic;

namespace DiffMatchPatch
{
    internal static class Extensions
    {
        internal static void Splice<T>(this IList<T> input, int start, int count, params T[] objects)
            => input.Splice(start, count, (IEnumerable<T>)objects);

        /// <summary>
        /// replaces [count] entries starting at index [start] with the given [objects]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="objects"></param>
        internal static void Splice<T>(this IList<T> input, int start, int count, IEnumerable<T> objects)
        {
            if (input is List<T> list)
            {
                list.RemoveRange(start, count);
                list.InsertRange(start, objects);

                return;
            }

            if (input is ListTailSegment<T> tailSegment)
            {
                tailSegment.RemoveRange(start, count);
                tailSegment.InsertRange(start, objects);

                return;
            }

            throw new NotImplementedException("todo cleanup this code");
        }
    }
}
