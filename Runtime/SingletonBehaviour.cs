//-----------------------------------------------------------------------
// <copyright file="SingletonBehaviour.cs" company="Lina Adkins">
//     Copyright (c) 2020 Lina Adkins and contributors. All rights reserved.
//     Licensed under MIT license. See LICENSE file in the project root for details.
// </copyright>
// <author>Lina Adkins</author>
//-----------------------------------------------------------------------

namespace TrigraphLabs.Patterns
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Singleton class that has the following features:
    /// * Obeys application quit rules, generally.
    /// * Is thread safe
    /// * Fully unit tested
    /// * Support for latest LTS as of 2019.4.11f11
    /// * Works with Unity's updated PlayMode optimizations
    /// New classes are created with 'public class YourClass : SingletonBehaviour&lt;YourClass\&gt; {}'
    /// It is recommend that you call YourClass.Instance while still on the main thread to avoid issues with accessing unity engine off of the main thread.
    /// </summary>
    /// <typeparam name="T">The name of your class derived from singleton.</typeparam>
    public abstract class SingletonBehaviour<T> : SingletonBehaviourBase where T : MonoBehaviour
    {
        #region Fields

        /// <summary> Thread Lock </summary>
        private static readonly object instanceLock = new object();
        private static readonly object persistentLock = new object();

        /// <summary> Stores the instance of our object as a static field. </summary>
        private static T _instance = null;

        /// <summary> Private field for Persistent property </summary>
        private static bool _persistent = false;

        #endregion

        #region Properties

        /// <summary> Gets or sets a value indicating whether our singleton instance is accessible.</summary>
        protected static bool IsAccessible { get; set; } = true;

        /// <summary> Toggle whether a scene load will destroy our singleton instance or not. Default behavior is non-persistent. This should only be set from the main thread. </summary>
        protected static bool IsPersistent
        {
            get { return _persistent; }
            set
            {
                // Add persistence
                if (value && !_persistent)
                {
                    if (_instance)
                    {
                        DontDestroyOnLoad(_instance);
                        _persistent = value;
                    }
                }
                else if (!value && _persistent) // Set to non persistent
                {
                    if (_instance)
                    {
                        SceneManager.MoveGameObjectToScene(_instance.gameObject, SceneManager.GetActiveScene());
                        _persistent = value;
                    }
                }
            }
        }

        /// <summary> Gets our singleton instance if accessible. Removes any extra instances from the scene with an error if they are present.</summary>
        public static T Instance
        {
            get
            {
                // RETURN NULL : If we are flagged as inaccessible, return null. Generally this only happens if the app is quitting, so make sure to check your instance reference for null OnApplicationQuit/OnDestroy.
                if (!IsAccessible) { return null; }

                // Double lock check for thread safety
                if (_instance == null)
                {
                    lock (instanceLock)
                    {
                        if (_instance == null)
                        {
                            // RETURN : If the object doesn't have an instance, we must check the scene. This portion of the code is not accessible from outside the main thread.
                            return GetInstanceFromScene();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary> Converts class name into printable string of the format "SingletonBehaviour<YourSingleton>".
        public static string ClassName
        {
            get
            {
                return $"{nameof(SingletonBehaviour<T>)}<{typeof(T)}>";
            }
            private set { }
        }

        #endregion

        #region Methods

        // <summary> Override this method instead of OnApplicationQuit() as SingletonBehaviour makes use of OnApplicationQuit() for access management.
        protected virtual void OnSingletonApplicationQuit()
        {
            // Empty
        }

        /// <summary> Destroy all existing instances of the singleton object in the scene. This should never return null. </summary>
        /// <returns> A single instance from the scene. </returns>
        private static T GetInstanceFromScene()
        {
            var instances = FindObjectsOfType<T>();
            var count = instances.Length;

            // We only have one in the scene, so use that one!
            if (count == 1)
            {
                _instance = instances[0];
                return _instance;
            }

            // We have more than one, log error and delete all but one.
            if (count > 1)
            {
                for (var i = 1; i < instances.Length; i++)
                {
                    GameObject.Destroy(instances[i]);
                }

                _instance = instances[0];
                return _instance;
            }

            // If we've reached here, no instances are found, create one.
            _instance = new GameObject($"({nameof(SingletonBehaviour<T>)}){typeof(T)}").AddComponent<T>();
            return _instance;
        }

        /// <summary>
        /// Hook into OnApplicationQuit to make singleton inaccessible. We do this to avoid 'phantom' objects from the singleton being called while the application is quitting. 
        /// This won't work in some instances:
        /// * WebGL and browser windows in general
        /// * Windows store apps and windows phone 8
        /// * iOS if Exit on suspend is not set in player settings.
        /// </summary>
        protected override sealed void OnApplicationQuit()
        {
            IsAccessible = false;
            this.OnSingletonApplicationQuit();
            // Help
        }

        #endregion
    }

    /// <summary>
    /// A singleton container that allows nicer syntax for the singleton template function.
    /// </summary>
    public abstract class SingletonBehaviourBase : MonoBehaviour
    {
        protected virtual void OnApplicationQuit() { /*empty*/ }
    }
}