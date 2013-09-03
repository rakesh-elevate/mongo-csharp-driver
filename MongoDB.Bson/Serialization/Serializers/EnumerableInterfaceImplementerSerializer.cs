﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for a class that implements IEnumerable.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class EnumerableInterfaceImplementerSerializer<TValue> :
        EnumerableInterfaceImplementerSerializerBase<TValue>,
        IBsonSerializerWithConfigurableChildSerializer,
        IBsonSerializerWithItemSerializer
            where TValue : class, IList, new()
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableInterfaceImplementerSerializer{TValue}"/> class.
        /// </summary>
        public EnumerableInterfaceImplementerSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableInterfaceImplementerSerializer{TValue}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public EnumerableInterfaceImplementerSerializer(IBsonSerializer itemSerializer)
            : base(itemSerializer)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public EnumerableInterfaceImplementerSerializer<TValue> WithItemSerializer(IBsonSerializer itemSerializer)
        {
            if (itemSerializer == ItemSerializer)
            {
                return this;
            }
            else
            {
                return new EnumerableInterfaceImplementerSerializer<TValue>(itemSerializer);
            }
        }

        // protected methods
        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new TValue();
        }

        // explicit interface implementations
        IBsonSerializer IBsonSerializerWithConfigurableChildSerializer.ConfigurableChildSerializer
        {
            get { return ItemSerializer; }
        }

        IBsonSerializer IBsonSerializerWithConfigurableChildSerializer.WithReconfiguredChildSerializer(IBsonSerializer childSerializer)
        {
            return WithItemSerializer(childSerializer);
        }

        IBsonSerializer IBsonSerializerWithItemSerializer.WithItemSerializer(IBsonSerializer itemSerializer)
        {
            return WithItemSerializer(itemSerializer);
        }
    }

    /// <summary>
    /// Represents a serializer for a class that implementes <see cref="IEnumerable{TItem}"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class EnumerableInterfaceImplementerSerializer<TValue, TItem> : 
        EnumerableInterfaceImplementerSerializerBase<TValue, TItem>,
        IBsonSerializerWithConfigurableChildSerializer,
        IBsonSerializerWithItemSerializer<EnumerableInterfaceImplementerSerializer<TValue, TItem>, TValue, TItem>
            where TValue : class, IEnumerable<TItem>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableInterfaceImplementerSerializer{TValue, TItem}"/> class.
        /// </summary>
        public EnumerableInterfaceImplementerSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableInterfaceImplementerSerializer{TValue, TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public EnumerableInterfaceImplementerSerializer(IBsonSerializer<TItem> itemSerializer)
            : base(itemSerializer)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public EnumerableInterfaceImplementerSerializer<TValue, TItem> WithItemSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            if (itemSerializer == ItemSerializer)
            {
                return this;
            }
            else
            {
                return new EnumerableInterfaceImplementerSerializer<TValue, TItem>(itemSerializer);
            }
        }

        // protected methods
        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new List<TItem>();
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The final result.</returns>
        protected override TValue FinalizeResult(object accumulator)
        {
            // find and call a constructor that we can pass the accumulator to
            var accumulatorType = accumulator.GetType();
            foreach (var constructorInfo in typeof(TValue).GetConstructors())
            {
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(accumulatorType))
                {
                    return (TValue)constructorInfo.Invoke(new object[] { accumulator });
                }
            }

            // otherwise try to find a no-argument constructor and an Add method
            var noArgumentConstructorInfo = typeof(TValue).GetConstructor(new Type[] { });
            var addMethodInfo = typeof(TValue).GetMethod("Add", new Type[] { typeof(TItem) });
            if (noArgumentConstructorInfo != null && addMethodInfo != null)
            {
                var value = (TValue)noArgumentConstructorInfo.Invoke(new Type[] { });
                foreach (var item in (IEnumerable<TItem>)accumulator)
                {
                    addMethodInfo.Invoke(value, new object[] { item });
                }
                return value;
            }

            var message = string.Format("Type '{0}' does not have a suitable constructor or Add method.", typeof(TValue).FullName);
            throw new BsonSerializationException(message);
        }

        // explicit interface implementations
        IBsonSerializer IBsonSerializerWithConfigurableChildSerializer.ConfigurableChildSerializer
        {
            get { return ItemSerializer; }
        }

        IBsonSerializer IBsonSerializerWithConfigurableChildSerializer.WithReconfiguredChildSerializer(IBsonSerializer childSerializer)
        {
            return WithItemSerializer((IBsonSerializer<TItem>)childSerializer);
        }

        IBsonSerializer IBsonSerializerWithItemSerializer.ItemSerializer
        {
            get { return ItemSerializer; }
        }

        IBsonSerializer IBsonSerializerWithItemSerializer.WithItemSerializer(IBsonSerializer itemSerializer)
        {
            return WithItemSerializer((IBsonSerializer<TItem>)itemSerializer);
        }
    }
}