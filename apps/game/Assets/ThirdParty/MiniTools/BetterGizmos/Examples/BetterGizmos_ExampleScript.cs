/////
/// Here is an example script to show some examples of how BetterGizmos functions can be called.
/// Remember that all the BetterGizmos functions are meant to be used within OnDrawGizmos or OnDrawGizmosSelected.
/// If one or multiple BetterGizmos functions are called outside of a UNITY_EDITOR define, you may get errors while trying to build.
/////

namespace MiniTools.BetterGizmos.Example
{
    using UnityEngine;
    using MiniTools.BetterGizmos; // Use this namespace to call BetterGizmos functions easier

#if UNITY_EDITOR
    using UnityEditor;
    [InitializeOnLoad]
#endif

    public class BetterGizmos_ExampleScript : MonoBehaviour
    {
        [Space(10)]
        [Header("GENERAL")]
        [SerializeField] private Transform helperText = null;

        [Space]
        [SerializeField] private Color gizmosColor = Color.red;
        [SerializeField] private Color castHitColor = Color.yellow;


        [Space(10)]
        [Header("----- ARROWS -----")]
        [SerializeField] private float arrowsSize = 1f;

        [Space]
        [SerializeField] private Transform freeArrowTarget = null;
        [SerializeField] private BetterGizmos.DownsizeDisplay freeArrowDownsizeDisplay = BetterGizmos.DownsizeDisplay.Squash;
        [SerializeField] private BetterGizmos.UpsizeDisplay freeArrowUpsizeDisplay = BetterGizmos.UpsizeDisplay.Offset;


        [Space(10)]
        [Header("----- JOYSTICK -----")]
        [SerializeField] private Color joystickDeadZoneColor = Color.gray;
        [SerializeField] private float joystickDisplaySize = 1f;

        [Space]
        [SerializeField] private Transform freeJoystick = null;
        [SerializeField][Range(0f, 1f)] private float joystickDeadZoneThreshold = 0.2f;

        [Space]
        [SerializeField] private Transform joystickCharacterView01 = null;

        [Space]
        [SerializeField] private Transform character02Ground = null;
        [SerializeField] private Transform joystickCharacterView02 = null;

        [Space]
        [SerializeField] private Transform joystickCharacterView03 = null;


        [Space(10)]
        [Header("----- RAYCAST -----")]
        [SerializeField] private float raycastDisplaySize = 1f;
        [SerializeField] private Transform linecastTarget = null;


        [Space(10)]
        [Header("----- BOX -----")]
        [SerializeField] private Vector3 boxScale = Vector3.one;

        [Space]
        [SerializeField] private BoxCollider boxCollider = null;

        [Space]
        [SerializeField] private float boxcastDisplaySize = 1f;
        [SerializeField] private BoxCollider boxcastCollider = null;


        [Space(10)]
        [Header("----- SPHERE -----")]
        [SerializeField] private float sphereRadius = 0.5f;

        [Space]
        [SerializeField] private SphereCollider sphereCollider = null;

        [Space]
        [SerializeField] private float spherecastDisplaySize = 1f;
        [SerializeField] private SphereCollider spherecastCollider = null;


        [Space(10)]
        [Header("----- CAPSULE -----")]
        [SerializeField] private float capsuleRadius = 0.5f;

        [Space]
        [SerializeField] private CapsuleCollider capsuleCollider = null;

        [Space]
        [SerializeField] private float capsulecastDisplaySize = 1f;
        [SerializeField] private CapsuleCollider capsulecastCollider = null;


        [Space(10)]
        [Header("----- RAYCAST 2D -----")]
        [SerializeField] private float raycast2DDisplaySize = 1f;
        [SerializeField] private Transform linecast2DTarget = null;


        [Space(10)]
        [Header("----- BOX 2D -----")]
        [SerializeField] private Vector3 box2DScale = Vector3.one;

        [Space]
        [SerializeField] private BoxCollider2D boxCollider2D = null;

        [Space]
        [SerializeField] private float boxcast2DDisplaySize = 1f;
        [SerializeField] private BoxCollider2D boxcastCollider2D = null;


        [Space(10)]
        [Header("----- CIRCLE 2D -----")]
        [SerializeField] private float circleRadius = 0.5f;

        [Space]
        [SerializeField] private CircleCollider2D circleCollider2D = null;

        [Space]
        [SerializeField] private float circlecastDisplaySize = 1f;
        [SerializeField] private CircleCollider2D circlecastCollider2D = null;


        [Space(10)]
        [Header("----- CAPSULE 2D -----")]
        [SerializeField] private Vector2 capsule2DSize = new Vector2(1f, 2f);
        [SerializeField] private CapsuleDirection2D capsule2DDirection = CapsuleDirection2D.Vertical;

        [Space]
        [SerializeField] private CapsuleCollider2D capsuleCollider2D = null;

        [Space]
        [SerializeField] private float capsulecast2DDisplaySize = 1f;
        [SerializeField] private CapsuleCollider2D capsulecastCollider2D = null;


#if UNITY_EDITOR

        #region EDITOR STUFF

        private BetterGizmos_ExampleScript()
        {
            // This ensures that objects are always updated, even if the application is not running
            EditorApplication.update += RefreshScene;
        }

        private void RefreshScene()
        {
            Quaternion characterViewRotation = Quaternion.Euler(0, Time.realtimeSinceStartup * 32f, 0);

            if (joystickCharacterView01)
                joystickCharacterView01.localRotation = characterViewRotation;

            if (joystickCharacterView02)
                joystickCharacterView02.localRotation = characterViewRotation;

            if (joystickCharacterView03)
                joystickCharacterView03.localRotation = characterViewRotation;
        }

        private void DrawHelper()
        {
            if (!Camera.current)
                return;

            if (Camera.current.transform.position.z > -3)
                return;

            bool selected = Selection.activeGameObject == gameObject;
            Color discColor = new Color(0, selected ? 0.5f : 0, 1, 0.25f);
            Color wireDiscColor = selected ? Color.cyan : Color.blue;

            Plane plane = new Plane(Vector3.back, new Vector3(0, 0, -3));
            Ray ray = new Ray(Camera.current.transform.position, Camera.current.transform.forward);
            plane.Raycast(ray, out float enter);
            Vector3 helperPos = ray.origin + ray.direction * enter - Vector3.right * 2.5f;
            helperPos = new Vector3(Mathf.Clamp(helperPos.x, -3.5f, 146), -0.5f, -3);

            Handles.color = discColor;
            Handles.DrawSolidDisc(helperPos, Vector3.forward, 0.5f);
            Handles.color = wireDiscColor;
            Handles.DrawWireDisc(helperPos, Vector3.forward, 0.5f);

            Gizmos.color = Color.clear;
            Gizmos.DrawSphere(helperPos, 0.5f);

            if (helperText)
                helperText.position = helperPos + new Vector3(0.75f, 0f, -0.1f);
        }

        private void OnValidate()
        {
            // If the user is using a render pipeline asset, automatically change the material of all the MehRenderers in the scene with a quick hack (assuming the user won't change the scene materials)
            if (!UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline)
                return;

            // Check if the URP shader has been set already
            if (GetComponent<MeshRenderer>().sharedMaterial.shader == UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultShader)
                return;

            Material URPMaterial = new Material(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultShader) { color = Color.white }; // Create a new material from the default URP shader and make it brighter
            MeshRenderer[] sceneRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < sceneRenderers.Length; i++)
                sceneRenderers[i].sharedMaterial = URPMaterial;
        }

        #endregion


        private void OnDrawGizmos()
        {
            // The helper is simply meant to help you select this script easily in the scene view :)
            // It is not related to BetterGizmos
            DrawHelper();

            // Arrow and joysticks
            DrawArrows();
            DrawJoysticks();

            // 3D Physics
            DrawRaycasts();
            DrawBoxes();
            DrawSpheres();
            DrawCapsules();

            // 2D Physics
            DrawRaycasts2D();
            DrawBoxes2D();
            DrawCircles();
            DrawCapsules2D();
        }


        #region ARROWS AND JOYSTICKS EXAMPLES

        private void DrawArrows()
        {
            // Facing
            Vector3 arrowBottomPos = Vector3.right * 5;
            BetterGizmos.DrawArrow(gizmosColor, arrowBottomPos, arrowBottomPos + Vector3.up * 2, Vector3.forward, arrowsSize, BetterGizmos.DownsizeDisplay.Squash, BetterGizmos.UpsizeDisplay.Offset);

            arrowBottomPos += Vector3.right;
            BetterGizmos.DrawViewFacingArrow(gizmosColor, arrowBottomPos, arrowBottomPos + Vector3.up * 2, arrowsSize, BetterGizmos.DownsizeDisplay.Squash, BetterGizmos.UpsizeDisplay.Offset);

            // Stretching
            float animatedHeight = 1 + Mathf.Cos(Time.realtimeSinceStartup * 3);

            arrowBottomPos += Vector3.right * 3;
            BetterGizmos.DrawArrow(gizmosColor, arrowBottomPos, arrowBottomPos + Vector3.up * animatedHeight, Vector3.forward, arrowsSize, BetterGizmos.DownsizeDisplay.Scale, BetterGizmos.UpsizeDisplay.Offset);

            arrowBottomPos += Vector3.right;
            BetterGizmos.DrawArrow(gizmosColor, arrowBottomPos, arrowBottomPos + Vector3.up * animatedHeight, Vector3.forward, arrowsSize, BetterGizmos.DownsizeDisplay.Scale, BetterGizmos.UpsizeDisplay.Stretch);

            arrowBottomPos += Vector3.right;
            BetterGizmos.DrawArrow(gizmosColor, arrowBottomPos, arrowBottomPos + Vector3.up * animatedHeight, Vector3.forward, arrowsSize, BetterGizmos.DownsizeDisplay.Squash, BetterGizmos.UpsizeDisplay.Offset);

            arrowBottomPos += Vector3.right;
            BetterGizmos.DrawArrow(gizmosColor, arrowBottomPos, arrowBottomPos + Vector3.up * animatedHeight, Vector3.forward, arrowsSize, BetterGizmos.DownsizeDisplay.Squash, BetterGizmos.UpsizeDisplay.Stretch);

            // Free arrow
            arrowBottomPos += Vector3.right * 3;

            if (freeArrowTarget)
                BetterGizmos.DrawViewFacingArrow(gizmosColor, arrowBottomPos, freeArrowTarget.position, arrowsSize, freeArrowDownsizeDisplay, freeArrowUpsizeDisplay);
        }

        private void DrawJoysticks()
        {
            // Free joysick
            // In a normal case, simply pass in the function the axis input of your controller
            Vector3 joystickPos = new Vector3(22, 0, -1);
            Vector2 joystickAxis = freeJoystick ? new Vector2(freeJoystick.transform.position.x - joystickPos.x, freeJoystick.transform.position.z - joystickPos.z) : Vector2.up;
            BetterGizmos.DrawJoystickAxis(gizmosColor, joystickDeadZoneColor, joystickDisplaySize, joystickPos, joystickAxis, joystickDeadZoneThreshold);

            // Simple rotation
            joystickPos += Vector3.right * 4;
            float rotationOffset = joystickCharacterView01 ? joystickCharacterView01.eulerAngles.y : 0f;

            BetterGizmos.DrawJoystickAxis(gizmosColor, joystickDeadZoneColor, joystickDisplaySize, joystickPos, joystickAxis, joystickDeadZoneThreshold, rotationOffset);

            // Complex rotations
            joystickPos += Vector3.right * 4 + Vector3.up * 0.5f;
            Vector3 character02GroundNormal = character02Ground ? character02Ground.up : Vector3.up;
            Vector3 character02Forward = joystickCharacterView02 ? joystickCharacterView02.forward : Vector3.forward;

            BetterGizmos.DrawJoystickAxis(gizmosColor, joystickDeadZoneColor, joystickDisplaySize, joystickPos, joystickAxis, joystickDeadZoneThreshold, character02GroundNormal, character02Forward);

            BetterGizmos.DrawJoystickAxis(gizmosColor, joystickDeadZoneColor, joystickDisplaySize, joystickAxis, joystickDeadZoneThreshold, joystickCharacterView03);
        }

        #endregion


        #region 3D PHYSICS EXAMPLES

        private void DrawRaycasts()
        {
            Vector3 startPoint = new Vector3(41, 2, -1);
            BetterGizmos.Raycast(gizmosColor, castHitColor, raycastDisplaySize, startPoint, Vector3.down, 4);

            startPoint += Vector3.right * 4;
            Vector3 endPoint = linecastTarget ? linecastTarget.position : startPoint - Vector3.up * 4;
            BetterGizmos.Linecast(gizmosColor, castHitColor, raycastDisplaySize, startPoint, endPoint);
        }

        private void DrawBoxes()
        {
            // Simple box
            Vector3 boxPos = new Vector3(52, 1.5f, -1);
            BetterGizmos.DrawBox(gizmosColor, boxPos, Quaternion.identity, boxScale);

            // BoxCollider
            if (boxCollider)
                BetterGizmos.DrawBox(gizmosColor, boxCollider);

            // Boxcast, using a BoxCollider
            if (boxcastCollider)
                BetterGizmos.Boxcast(gizmosColor, castHitColor, boxcastDisplaySize, boxcastCollider, Vector3.forward, 6);
        }

        private void DrawSpheres()
        {
            // Simple sphere
            Vector3 spherePos = new Vector3(68, 1.5f, -1);
            BetterGizmos.DrawSphere(gizmosColor, spherePos, sphereRadius);

            // SphereCollider
            if (sphereCollider)
                BetterGizmos.DrawSphere(gizmosColor, sphereCollider);

            // Spherecast, using a SphereCollider
            if (spherecastCollider)
                BetterGizmos.Spherecast(gizmosColor, castHitColor, spherecastDisplaySize, spherecastCollider, Vector3.forward, 6);
        }

        private void DrawCapsules()
        {
            // Simple capsule
            Vector3 capsulePos = new Vector3(84, 1.5f, -1);
            BetterGizmos.DrawCapsule(gizmosColor, capsulePos, Vector3.up, 2, capsuleRadius);

            // CapsuleCollider
            if (capsuleCollider)
                BetterGizmos.DrawCapsule(gizmosColor, capsuleCollider);

            // Capsulecast, using a CapsuleCollider
            if (capsulecastCollider)
                BetterGizmos.Capsulecast(gizmosColor, castHitColor, capsulecastDisplaySize, capsulecastCollider, Vector3.forward, 6);
        }

        #endregion


        #region 2D PHYSICS EXAMPLES

        private void DrawRaycasts2D()
        {
            Vector2 startPoint = new Vector3(100, 2);
            BetterGizmos.Raycast2D(gizmosColor, castHitColor, raycast2DDisplaySize, startPoint, Vector3.down, 4);

            startPoint += Vector2.right * 4;
            Vector2 endPoint = linecast2DTarget ? (Vector2)linecast2DTarget.position : startPoint - Vector2.up * 4;
            BetterGizmos.Linecast2D(gizmosColor, castHitColor, raycast2DDisplaySize, startPoint, endPoint);
        }

        private void DrawBoxes2D()
        {
            // Simple box
            Vector3 boxPos = new Vector3(111, 1.5f);
            BetterGizmos.DrawBox2D(gizmosColor, boxPos, box2DScale, 0);

            // BoxCollider2D
            if (boxCollider2D)
                BetterGizmos.DrawBox2D(gizmosColor, boxCollider2D);

            // 2D Boxcast, using a BoxCollider2D
            if (boxcastCollider2D)
                BetterGizmos.Boxcast2D(gizmosColor, castHitColor, boxcast2DDisplaySize, boxcastCollider2D, Vector3.down, 4);
        }

        private void DrawCircles()
        {
            // Simple circle
            Vector2 circlePos = new Vector2(126, 1.5f);
            BetterGizmos.DrawCircle2D(gizmosColor, circlePos, Vector3.forward, circleRadius);

            // CircleCollider2D
            if (circleCollider2D)
                BetterGizmos.DrawCircle2D(gizmosColor, circleCollider2D);

            // Circlecast, using a CircleCollider2D
            if (circlecastCollider2D)
                BetterGizmos.Circlecast2D(gizmosColor, castHitColor, circlecastDisplaySize, circlecastCollider2D, Vector2.down, 4);
        }

        private void DrawCapsules2D()
        {
            // Simple capsule
            Vector3 capsulePos = new Vector3(141, 1.5f);
            BetterGizmos.DrawCapsule2D(Color.red, capsulePos, capsule2DSize, 0, capsule2DDirection);

            // CapsuleCollider2D
            if (capsuleCollider2D)
                BetterGizmos.DrawCapsule2D(Color.red, capsuleCollider2D);

            // Capsulecast, using a CapsuleCollider2D
            if (capsulecastCollider2D)
                BetterGizmos.Capsulecast2D(gizmosColor, castHitColor, capsulecast2DDisplaySize, capsulecastCollider2D, Vector3.down, 4);
        }

        #endregion

#endif
    }
}