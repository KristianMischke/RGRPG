using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
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

        //Takes object in queue and puts in list (and activates it). If no inactive create new.
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

        // Takes object in active and deactivates it.
        public void Deactivate(T obj) {
            deactivateAction(obj);
            inactiveObjects.Enqueue(obj);
        } 
    }
}
