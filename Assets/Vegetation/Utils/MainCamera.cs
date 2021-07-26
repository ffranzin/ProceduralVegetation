using UnityEngine;


namespace Utils.Cameras
{
    public class MainCamera : Singleton<MainCamera>
    {
        public Plane[] frustumPlanes { get; private set; }
        public Vector4[] frustumPlanesNormals { get; private set; }

        private Camera m_Camera;
        public Camera Camera
        {
            get
            {
                return m_Camera;
            }
            set
            {
                m_Camera = value;
                UpdateFrustumPlanes();
            }
        }

        void Awake()
        {
            m_Camera = Camera.main;

            frustumPlanes = new Plane[6];
            frustumPlanesNormals = new Vector4[6];

            base.Awake();
        }

        private void UpdateFrustumPlanes()
        {
            GeometryUtility.CalculateFrustumPlanes(m_Camera, frustumPlanes);

            for (int i = 0; i < 6; i++)
            {
                frustumPlanesNormals[i] = frustumPlanes[i].normal;
            }
        }


        private void FixedUpdate()
        {
            UpdateFrustumPlanes();
        }
    }
}