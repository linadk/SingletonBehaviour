//-----------------------------------------------------------------------
// <copyright file="SingletonTests.cs" company="Lina Adkins">
//     Copyright (c) 2020 Lina Adkins and contributors. All rights reserved.
//     Licensed under MIT license. See LICENSE file in the project root for details.
// </copyright>
// <author>Lina Adkins</author>
//-----------------------------------------------------------------------

namespace TrigraphLabs.Patterns.Tests
{
    using NUnit.Framework;
    using System.Collections;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;

    using TrigraphLabs.Patterns;

    /// <summary>
    /// Singleton class test fixture.
    /// </summary>
    [TestFixture]
    public class SingletonTests
    {
        /// <summary> Clear out all singleton GameObjects. </summary>
        [UnityTearDown]
        public IEnumerator Teardown()
        {
            foreach (var s in GameObject.FindObjectsOfType<TestSingleton>())
            {
                GameObject.DestroyImmediate(s);
            }

            yield return null;
        }

        /// <summary> Set defaults, generate too many objects. </summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TestSingleton.SetIsAccessible(true);
            TestSingleton.SetPersistence(false);
            CreateMultipleSingletonObjects(Random.Range(5, 25), true);
            yield return 0;
        }

        /// <summary>Ensure we get a valid instance even with multiple objects.</summary>
        [UnityTest]
        public IEnumerator Get_Instance_WithMultipleObjects()
        {
            // Arrange
            // None

            // Act
            var instance = TestSingleton.Instance;

            yield return null;

            // Assert
            Assert.IsNotNull(instance);


        }

        /// <summary>Assert that the singleton logic will reduce its number of GameObject instances to one.</summary>
        [UnityTest]
        public IEnumerator Get_Instance_DoesCleanupObjects()
        {
            // Arrange
            // None

            // Act
            var instance = TestSingleton.Instance; // This should remove all instances

            yield return null; // Wait for next frame

            // Assert
            Assert.Less(GameObject.FindObjectsOfType<TestSingleton>().Length, 2);
        }

        /// <summary>See that OnApplicationQuit processes properly to avoid phantom objects.</summary>
        [UnityTest]
        public IEnumerator OnApplicationQuitBehaves()
        {
            // Arrange
            var instance = TestSingleton.Instance;

            // Act
            GameObject.DestroyImmediate(instance.gameObject);

            instance.MockApplicationQuit(); // Mimic application quitting, should make instance inaccessible

            yield return null;

            // Assert
            Assert.IsNull(TestSingleton.Instance, "Singleton creating instances after application quit called!");
        }

        /// <summary>Assert that non persistent objects do not persist through scene loads.</summary>
        [UnityTest]
        public IEnumerator NonPersistentSceneChange()
        {
            // Arrange
            var instance = TestSingleton.Instance;

            // Act
            TestSingleton.SetPersistence(false);

            var asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Assert
            Assert.AreEqual(0, GameObject.FindObjectsOfType<TestSingleton>().Length);
        }

        /// <summary> Assert that persistent objects remain through scene load. </summary>
        [UnityTest]
        public IEnumerator PersistentSceneChange()
        {
            // Arrange
            var instance = TestSingleton.Instance;

            // Act
            TestSingleton.SetPersistence(true);

            var asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Assert
            Assert.AreEqual(1, GameObject.FindObjectsOfType<TestSingleton>().Length);
        }

        ///<summary>Assert that we get a valid instance from a non-main thread.</summary>
        [UnityTest]
        public IEnumerator Get_Instance_Threaded()
        {
            Thread t;
            t = new Thread(ThreadedFunc);
            yield return new WaitForSeconds(0.01f); // We wait to avoid the object being destroyed by teardown
        }

        ///<summary>Thread function we used to test threading.</summary>
        private void ThreadedFunc()
        {
            var instance = TestSingleton.Instance;
            Assert.IsNotNull(instance);
            return;
        }

        /// <summary>
        /// Create a single singleton object for testing.
        /// </summary>
        private GameObject CreateSingletonObject(string name)
        {
            var go = new GameObject(name);
            go.AddComponent(typeof(TestSingleton));
            return go;
        }

        /// <summary>
        /// Create multiple singleton objects for testing.
        /// </summary>
        /// <param name="numObjects"></param>
        private void CreateMultipleSingletonObjects(int numObjects, bool randomData = false)
        {
            for (int i = 0; i < numObjects; i++)
            {
                CreateSingletonObject("SingletonObject" + i.ToString());
            }

            if (!randomData) { return; }
            TestSingleton.SetPersistence(Random.value > 0.5f); // Randomly assign persistence
        }

        /// <summary>
        /// Simple test object to test singleton functionality.
        /// </summary>
        private class TestSingleton : SingletonBehaviour<TestSingleton>
        {
            /// <summary>
            /// Override on destroy just so we can test app quit.
            /// </summary>
            public void MockApplicationQuit()
            {
                OnApplicationQuit();
            }

            protected override void OnSingletonApplicationQuit()
            {
            }

            /// <summary>
            /// Accessor for testing.
            /// </summary>
            /// <param name="persistence"></param>
            static public void SetPersistence(bool persistence)
            {
                IsPersistent = persistence;
            }

            static public void SetIsAccessible(bool accessible)
            {
                IsAccessible = accessible;
            }
        }
    }
}
