using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarkusSecundus.Utils.Physics
{
    public class ColliderActivityTracker : MonoBehaviour
    {
        [SerializeField] bool UseTrigger;
        [SerializeField] bool UseCollision;

        HashSet<Collider>   _triggers;
        HashSet<Collider>   _collisions;
        HashSet<Collider2D> _triggers2D;
        HashSet<Collider2D> _collisions2D;

        private void Start()
        {
            StartCoroutine(cleanup());
            IEnumerator cleanup()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1.37f);
                    List<object> buffer = new();
                    _triggers?.RemoveAllDead(buffer);
                    _collisions?.RemoveAllDead(buffer);
                    _triggers2D?.RemoveAllDead(buffer);
                    _collisions2D?.RemoveAllDead(buffer);
                }
            }
        }

        public IReadOnlyCollection<Collider> ActiveTriggers => _triggers??= new();
        public IReadOnlyCollection<Collider2D> ActiveTriggers2D => _triggers2D ??= new();
        public IReadOnlyCollection<Collider> ActiveCollisions => _collisions ??= new();
        public IReadOnlyCollection<Collider2D> ActiveCollisions2D => _collisions2D ??= new();

        List<IColliderActivityInfo> _listeners;

        public void RegisterListener(IColliderActivityInfo listener)
        {
            _listeners ??= new();
            _listeners.Add(listener);
            if (UseTrigger && _triggers.IsNotNil())
            {
                foreach (var t in _triggers) listener.Enter(t);
                foreach (var t in _triggers2D) listener.Enter(t);
            }
            if (UseCollision && _collisions.IsNotNil())
            {
                foreach (var c in _collisions) listener.Enter(c);
                foreach (var c in _collisions2D) listener.Enter(c);
            }
        }
        public bool UnregisterListener(IColliderActivityInfo listener) => _listeners?.Remove(listener) == true;

        private void OnTriggerEnter(Collider other) { if (UseTrigger) ColliderEnter(other, ref _triggers); }
        private void OnTriggerEnter2D(Collider2D other) { if (UseTrigger) ColliderEnter(other, ref _triggers2D); }
        private void OnCollisionEnter(Collision collision) {if(UseCollision) ColliderEnter(collision.collider, ref _collisions); }
        private void OnCollisionEnter2D(Collision2D collision) { if (UseCollision) ColliderEnter(collision.collider, ref _collisions2D); }

        private void OnTriggerExit(Collider other) { if (UseTrigger) ColliderExit(other, _triggers); }
        private void OnTriggerExit2D(Collider2D other) { if (UseTrigger) ColliderExit(other, _triggers2D); }
        private void OnCollisionExit(Collision collision) { if (UseCollision) ColliderExit(collision.collider, _collisions); }
        private void OnCollisionExit2D(Collision2D collision) { if (UseCollision) ColliderExit(collision.collider, _collisions2D); }

        void ColliderEnter<TComponent>(TComponent c, ref HashSet<TComponent> set) where TComponent: Component
        {
            set ??= new();
            set.Add(c);
            foreach (var l in _listeners) l.Enter(c);
        }
        void ColliderExit<TComponent>(TComponent c, HashSet<TComponent> set) where TComponent : Component
        {
            set?.Remove(c);
            if (typeof(TComponent) == typeof(Collider))
            foreach (var l in _listeners) l.Exit(c);
        }
    }
}
