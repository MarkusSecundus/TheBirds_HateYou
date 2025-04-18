using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;


namespace MarkusSecundus.Utils.Behaviors.Automatization
{
    /// <summary>
    /// Component for spawning randomized entities inside a set area.
    /// 
    /// <para>
    /// Entities can be randomized by containing <see cref="IRandomizer"/> components.
    /// </para>
    /// </summary>
    public class PlaceObjectsOnPoints : MonoBehaviour
    {
        /// <summary>
        /// Set of points where the objects will be spawned.
        /// </summary>
        [SerializeField] public IPointsSupplier SpawnLocation;

        [SerializeField] public int MaxChildrenInExistence = -1;
        /// <summary>
        /// Root of the transform hierarchy where spawned objects will be placed
        /// </summary>
        public Transform ParentToFill;

        public int ChildrenCount => ParentToFill.transform.childCount;

        public Interval<int> GroupSize = new Interval<int>(1, 4);

        /// <summary>
        /// Prototype of the spawned object
        /// </summary>
        public GameObject ToPlace;

        /// <summary>
        /// Seed for the source of randomness that's newly generated on each call
        /// </summary>
        public int Seed = 0;
        /// <summary>
        /// If set to <c>true</c>, <see cref="Seed"/> field will be ignored - random seed will be generated each time.
        /// </summary>
        public bool RandomSeed = true;
        private int RealSeed => RandomSeed ? Random.Range(int.MinValue, int.MaxValue) : Seed;

        /// <summary>
        /// Spawn an object on every point provided by <see cref="SpawnLocation"/>.
        /// </summary>
        public void PlaceObjects() => PlaceObjects(SpawnLocation.IteratePoints());
        /// <summary>
        /// Spawn <paramref name="count"/> objects on random points of <see cref="SpawnLocation"/>
        /// </summary>
        /// <param name="count"></param>
        public void PlaceObjectsRandom(int count) => PlaceObjects(count, () => SpawnLocation.GetRandomPoint(new System.Random(RealSeed)));
        /// <summary>
        /// Spawn <paramref name="count"/> objects on random points inside <see cref="SpawnLocation"/>'s volume
        /// </summary>
        /// <param name="count"></param>
        public void PlaceObjectsRandomVolume(int count) => PlaceObjects(count, () => SpawnLocation.GetRandomPointInVolume(new System.Random(RealSeed)));
        private void PlaceObjects(int count, System.Func<Vector3> supplier) => PlaceObjects(supplier.Repeat(count));

        /// <summary>
        /// Clean all objects inside <see cref="ParentToFill"/> and then call <see cref="PlaceObjects"/>.
        /// </summary>
        public void ClearAndPlaceObjects()
        {
            ClearParent();
            PlaceObjects();
        }

        static System.Random rand = new System.Random();
        private void PlaceObjects(IEnumerable<Vector3> points, bool shouldClear = false)
        {
            //temp
            GroupSize = new Interval<int>(1, 4);

            if (MaxChildrenInExistence >= 0 && ChildrenCount >= MaxChildrenInExistence) return;

            var random = new System.Random(RealSeed);

            foreach (var v in points)
            {
                int groupSize = rand.Next(GroupSize.Min, GroupSize.Max);
                for(int i = 0; i < groupSize; i++)
                {
                    if (MaxChildrenInExistence >= 0 && ChildrenCount >= MaxChildrenInExistence) break;
                    var obj = ToPlace.InstantiateWithTransform();
                    obj.transform.position = v + ToPlace.transform.localPosition + random.NextVector3(new Interval<Vector3>(-Vector3.one*3, Vector3.one*3));
                    obj.transform.SetParent(ParentToFill);
                    obj.SetActive(true);
                    foreach (var randomizer in obj.GetComponentsInChildren<IRandomizer>())
                        randomizer.Randomize(random);
                }
            }
        }

        /// <summary>
        /// Destroy all game objects inside <see cref="ParentToFill"/>
        /// </summary>
        public void ClearParent()
        {
            while (ParentToFill.childCount > 0)
            {
                foreach (Transform t in ParentToFill)
                    Object.DestroyImmediate(t.gameObject);
            }
        }
    }
}