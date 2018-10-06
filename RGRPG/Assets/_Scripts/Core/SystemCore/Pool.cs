using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Class for storing objects that are in an "inactive" state and functionality for them to return to an "active" state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool <T>
    {
        private Queue<T> inactiveObjects;
        private Action<T> activateAction;
        private Action<T> deactivateAction;
        private Func<T> createNewObject;

       
        public Pool (Action<T> activate, Action<T> deactivate, Func<T> createNew){
            inactiveObjects = new Queue<T>();

            activateAction = activate;
            deactivateAction = deactivate;
            createNewObject = createNew;
        }

        /// <summary>
        ///     Gets an inactive object from the queue (and activates it), or if none exists creates a new object
        /// </summary>
        /// <returns>An activated object</returns>
        public T Get() {
            if (inactiveObjects.Count >= 1)
            {
               T objectTemp = inactiveObjects.Dequeue();
                activateAction(objectTemp);
                return objectTemp;
            }
            else
            {
                T newObject = createNewObject();
                return newObject;
            }
        }

        /// <summary>
        ///     Given an object, deactivates it and adds to the pool
        /// </summary>
        /// <param name="obj">The object to add to the pool</param>        
        public void Deactivate(T obj) {
            deactivateAction(obj);
            inactiveObjects.Enqueue(obj);
        } 
    }
}
