using UnityEngine;

namespace MiniTools
{
    namespace BetterGizmos
    {
        public static class VerticesTables
        {
            #region ARROW

            public static Vector3[] ArrowBottomVertices => arrowBottomVertices;
            private static Vector3[] arrowBottomVertices = new Vector3[]
            {
            new Vector3(0.125f, 0, 0.5f),
            new Vector3(0.125f, 0, 0),
            new Vector3(-0.125f, 0, 0),
            new Vector3(-0.125f, 0, 0.5f)
            };

            public static Vector3[] ArrowTopVertices => arrowTopVertices;
            private static Vector3[] arrowTopVertices = new Vector3[]
            {
            new Vector3(-0.125f, 0, -0.5f),
            new Vector3(-0.25f, 0, -0.5f),
            new Vector3(0, 0, 0),
            new Vector3(0.25f, 0, -0.5f),
            new Vector3(0.125f, 0, -0.5f)
            };

            #endregion

            #region CUBE

            public static Vector3[] CubeVertices => cubeVertices;
            private static Vector3[] cubeVertices = new Vector3[]
            {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f)
            };

            #endregion

            #region CIRCLE

            public static Vector3[] Circle32Vertices => circle32Vertices;
            private static Vector3[] circle32Vertices = new Vector3[]
            {
            new Vector3(0.4903920746f, 0.09754481316f, 0f),
            new Vector3(0.4619393158f, 0.1913412857f, 0f),
            new Vector3(0.4157344818f, 0.2777846146f, 0f),
            new Vector3(0.3535531616f, 0.3535528564f, 0f),
            new Vector3(0.2777850342f, 0.4157342148f, 0f),
            new Vector3(0.1913417435f, 0.4619392014f, 0f),
            new Vector3(0.09754528999f, 0.4903921127f, 0f),
            new Vector3(2.235174179e-07f, 0.4999995422f, 0f),
            new Vector3(-0.0975448513f, 0.4903922272f, 0f),
            new Vector3(-0.1913413429f, 0.4619394302f, 0f),
            new Vector3(-0.2777847099f, 0.4157345581f, 0f),
            new Vector3(-0.3535529709f, 0.3535532379f, 0f),
            new Vector3(-0.4157343674f, 0.2777850723f, 0f),
            new Vector3(-0.4619393539f, 0.1913417435f, 0f),
            new Vector3(-0.4903922653f, 0.09754526138f, 0f),
            new Vector3(-0.4999996567f, 1.639127731e-07f, 0f),
            new Vector3(-0.4903923798f, -0.09754495621f, 0f),
            new Vector3(-0.4619395828f, -0.1913414764f, 0f),
            new Vector3(-0.4157346725f, -0.2777848625f, 0f),
            new Vector3(-0.3535533142f, -0.3535531235f, 0f),
            new Vector3(-0.2777850914f, -0.4157345581f, 0f),
            new Vector3(-0.1913417244f, -0.4619395447f, 0f),
            new Vector3(-0.0975452137f, -0.4903924561f, 0f),
            new Vector3(-8.195638657e-08f, -0.4999998474f, 0f),
            new Vector3(0.09754506111f, -0.4903925323f, 0f),
            new Vector3(0.19134161f, -0.4619396973f, 0f),
            new Vector3(0.277784996f, -0.415734787f, 0f),
            new Vector3(0.3535533142f, -0.3535533905f, 0f),
            new Vector3(0.4157347488f, -0.2777851295f, 0f),
            new Vector3(0.4619397354f, -0.1913417053f, 0f),
            new Vector3(0.4903926086f, -0.09754516602f, 0f),
            new Vector3(0.5f, 0f, 0f),
            };

            public static Vector3[] Circle16Vertices => circle16Vertices;
            private static Vector3[] circle16Vertices = new Vector3[]
            {
            new Vector3(0.4619393158f, 0.1913412857f, 0f),
            new Vector3(0.3535531616f, 0.3535528564f, 0f),
            new Vector3(0.1913417435f, 0.4619392014f, 0f),
            new Vector3(2.235174179e-07f, 0.4999995422f, 0f),
            new Vector3(-0.1913413429f, 0.4619394302f, 0f),
            new Vector3(-0.3535529709f, 0.3535532379f, 0f),
            new Vector3(-0.4619393539f, 0.1913417435f, 0f),
            new Vector3(-0.4999996567f, 1.639127731e-07f, 0f),
            new Vector3(-0.4619395828f, -0.1913414764f, 0f),
            new Vector3(-0.3535533142f, -0.3535531235f, 0f),
            new Vector3(-0.1913417244f, -0.4619395447f, 0f),
            new Vector3(-8.195638657e-08f, -0.4999998474f, 0f),
            new Vector3(0.19134161f, -0.4619396973f, 0f),
            new Vector3(0.3535533142f, -0.3535533905f, 0f),
            new Vector3(0.4619397354f, -0.1913417053f, 0f),
            new Vector3(0.5f, 0f, 0f)
            };

            public static Vector3[] Circle8Vertices => circle8Vertices;
            private static Vector3[] circle8Vertices = new Vector3[]
            {
            new Vector3(0.3535531616f, 0.3535528564f, 0f),
            new Vector3(0f, 0.5f, 0f),
            new Vector3(-0.3535529709f, 0.3535532379f, 0f),
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(-0.3535533142f, -0.3535531235f, 0f),
            new Vector3(-0f, -0.5f, 0f),
            new Vector3(0.3535533142f, -0.3535533905f, 0f),
            new Vector3(0.5f, 0f, 0f)
            };

            #endregion
        }
    }
}