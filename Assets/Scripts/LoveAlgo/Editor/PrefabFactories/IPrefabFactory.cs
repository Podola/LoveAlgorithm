#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoveAlgo.Editor.PrefabFactories
{
    /// <summary>
    /// Defines the contract for prefab factory implementations that can be orchestrated by the PrefabFactoryManager.
    /// </summary>
    public interface IPrefabFactory
    {
        /// <summary>
        /// Friendly name used in UI/logs.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Output path used when saving the prefab asset.
        /// </summary>
        string OutputPath { get; }

        /// <summary>
        /// Declares dependencies so the manager can determine build order.
        /// </summary>
        IEnumerable<FactoryDependency> Dependencies { get; }

        /// <summary>
        /// Builds the prefab and returns the instantiated root GameObject.
        /// The caller is responsible for saving and destroying the result.
        /// </summary>
        GameObject BuildPrefab();
    }

    public readonly struct FactoryDependency
    {
        public FactoryDependency(Type factoryType, DependencyRequirement requirement = DependencyRequirement.Required)
        {
            FactoryType = factoryType;
            Requirement = requirement;
        }

        public Type FactoryType { get; }
        public DependencyRequirement Requirement { get; }
    }

    public enum DependencyRequirement
    {
        Required = 0,
        Optional = 1
    }
}
#endif
