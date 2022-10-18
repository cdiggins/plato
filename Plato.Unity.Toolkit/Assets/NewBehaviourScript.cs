using UnityEngine;

namespace Plato.Unity
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class MeshCloner : MonoBehaviour
    {
        public int Rows = 5;
        public int Columns = 5;
        public float Width = 10;
        public float Height = 10;

        public void Update()
        {
            var filter = GetComponent<MeshFilter>();
            if (filter == null)
                return;
            var mesh = filter.sharedMesh;

            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
                return;
            var material = renderer.sharedMaterial;


            for (var i = 0; i < Columns; ++i)
            {
                var x = i * Width / Columns;

                for (var j = 0; j < Rows; ++j)
                {
                    var y = j * Height / Rows;

                    var position = new Vector3(x, y);

                    var rotation = Quaternion.identity;

                    Graphics.DrawMesh(mesh, position, rotation, material, 0);
                }
            }
        }
    }
}