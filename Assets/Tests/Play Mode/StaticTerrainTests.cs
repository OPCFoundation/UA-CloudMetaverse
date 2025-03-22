using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace BladeMR.Tests
{
    public class StaticTerrainTests
    {
        private const string StaticTerrainPath = "Static Terrain";

        private GameObject staticTerrain;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return LoadStaticTerrainAsset();
        }

        //Load the static terrain model via Addressables
        private IEnumerator LoadStaticTerrainAsset()
        {
            var asyncHandle = Addressables.InstantiateAsync(StaticTerrainPath);
            yield return asyncHandle;
            staticTerrain = asyncHandle.Result;
            Assert.IsNotNull(staticTerrain);
        }

        [Test]
        public void HasHologramScale()
        {
            Assert.IsNotNull(staticTerrain);
            var scaler = staticTerrain.GetComponent<HologramScale>();
            Assert.IsNotNull(scaler);

            //Real world scale of terrain should be 1617m
            //https://unity.slack.com/archives/C01L64K8URE/p1614362182078600?thread_ts=1614354140.068300&cid=C01L64K8URE
            Assert.AreEqual(scaler.realWorldScale, 1617);
            Assert.AreEqual(scaler.hologramScale, 1);
        }

        [Test]
        public void HasObjectManipulator()
        {
        }
    }
}