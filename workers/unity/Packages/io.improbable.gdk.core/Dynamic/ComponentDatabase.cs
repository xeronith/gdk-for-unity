using System;
using System.Collections.Generic;
using System.Linq;
using Improbable.Gdk.Core.Commands;

namespace Improbable.Gdk.Core
{
    public static class ComponentDatabase
    {
        internal static Dictionary<uint, IComponentMetaclass> Metaclasses { get; }

        private static Dictionary<Type, uint> ComponentsToIds { get; }

        private static Dictionary<Type, uint> SnapshotsToIds { get; }

        private static Dictionary<Type, ICommandMetaclass> RequestsToCommandMetaclass { get; }

        static ComponentDatabase()
        {
            Metaclasses = ReflectionUtility.GetNonAbstractTypes(typeof(IComponentMetaclass))
                .Select(type => (IComponentMetaclass) Activator.CreateInstance(type))
                .ToDictionary(metaclass => metaclass.ComponentId, metaclass => metaclass);

            ComponentsToIds = Metaclasses.ToDictionary(pair => pair.Value.Data, pair => pair.Key);
            SnapshotsToIds = Metaclasses.ToDictionary(pair => pair.Value.Snapshot, pair => pair.Key);
            RequestsToCommandMetaclass = Metaclasses.Where(pair => pair.Value.Commands.Length > 0)
                .SelectMany(pair => pair.Value.Commands.Select(cmd => new KeyValuePair<Type, ICommandMetaclass>(cmd.Request, cmd)))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public static IComponentMetaclass GetMetaclass(uint componentId)
        {
            if (!Metaclasses.TryGetValue(componentId, out var metaclass))
            {
                throw new ArgumentException($"Can not find Metaclass for SpatialOS component ID {componentId}.");
            }

            return metaclass;
        }

        public static uint GetComponentId<T>() where T : ISpatialComponentData
        {
            if (!ComponentsToIds.TryGetValue(typeof(T), out var id))
            {
                throw new ArgumentException($"Can not find ID for unregistered SpatialOS component {nameof(T)}.");
            }

            return id;
        }

        public static uint GetComponentId(Type type)
        {
            if (!ComponentsToIds.TryGetValue(type, out var id))
            {
                throw new ArgumentException($"Can not find ID for unregistered SpatialOS component {type.Name}.");
            }

            return id;
        }

        public static uint GetSnapshotComponentId<T>() where T : ISpatialComponentSnapshot
        {
            if (!SnapshotsToIds.TryGetValue(typeof(T), out var id))
            {
                throw new ArgumentException($"Can not find ID for unregistered SpatialOS component snapshot {nameof(T)}.");
            }

            return id;
        }

        public static ICommandMetaclass GetRequestCommandMetaclass<T>() where T : ICommandRequest
        {
            if (!RequestsToCommandMetaclass.TryGetValue(typeof(T), out var metaclass))
            {
                throw new ArgumentException($"Can not find ID for unregistered SpatialOS command request {nameof(T)}.");
            }

            return metaclass;
        }
    }
}
