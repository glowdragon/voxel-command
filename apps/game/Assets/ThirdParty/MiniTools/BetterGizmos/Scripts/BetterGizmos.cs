namespace MiniTools
{
    namespace BetterGizmos
    {
        #if UNITY_EDITOR

        using UnityEngine;
        using System.Collections.Generic;
        using UnityEditor;

        [InitializeOnLoad]
        public static class BetterGizmos
        {
            static BetterGizmos ()
            {
                EditorApplication.update += UpdateAnimationParameters;
            }

            // Enums
            public enum UpsizeDisplay
            {
                Offset,
                Stretch
            }

            public enum DownsizeDisplay
            {
                Squash,
                Scale
            }

            // Gizmos cache
            private const float timeWaveScaleAmplitude = 0.1f;
            private static float timeWaveScaleMultiplier;

            private const float castHitAlphaMultiplier = 0.25f;
            private const float solidColorMultiplier = 0.138f;
            private const float convexShapeColorMultiplier = 0.25f;

            private static int maxConvexShapePoints = VerticesTables.Circle32Vertices.Length + 4; // Capsule shape: +2 / Rounded box 2D: +4
            private static List<Vector3> convexShapeList = new List<Vector3>(maxConvexShapePoints);
            private static List<Vector3> convexShapeScreenPointList = new List<Vector3>(maxConvexShapePoints);
            private static Vector3[] convexShapeArray = new Vector3[maxConvexShapePoints];

            private static Vector3[] capsule32PointsCloud = new Vector3[VerticesTables.Circle32Vertices.Length * 2];
            private static Vector3[] capsule16PointsCloud = new Vector3[VerticesTables.Circle16Vertices.Length * 2];
            private static Vector3[] capsule8PointsCloud = new Vector3[VerticesTables.Circle8Vertices.Length * 2];

            private static Vector3[] cubePointsCloud = new Vector3[VerticesTables.CubeVertices.Length];

            private static Vector3[] box2DPointsCloud = new Vector3[5];
            private static Vector3[] box2DRounded32PointsCloud = new Vector3[VerticesTables.Circle32Vertices.Length * 4];
            private static Vector3[] box2DRounded16PointsCloud = new Vector3[VerticesTables.Circle16Vertices.Length * 4];
            private static Vector3[] box2DRounded8PointsCloud = new Vector3[VerticesTables.Circle8Vertices.Length * 4];

            private static float highDefinitionThreshold = 1f;
            private static float midDefinitionThreshold = 4f;

            private static float sizeRatioDiscardThreshold = 0.01f;

            private static RaycastHit2D[] cast2DResults = new RaycastHit2D[1];


            #region ARROWS

            /// <summary>
            /// Draws an arrow always facing the given normal.
            /// </summary>
            /// <param name="startPoint">Start point of the arrow.</param>
            /// <param name="endPoint">End point of the arrow.</param>
            /// <param name="normal">Normal of the arrow.</param>
            /// <param name="arrowSize">Size of the arrow.</param>
            /// <param name="color">Color of the arrow.</param>
            /// <param name="selectable">Defines if the arrow is selectable or not.</param>
            /// <param name="downsizeDisplay">Defines how the arrow's display should behave when the distance between the start and end points is shorter than the arrow size.</param>
            /// <param name="upsizeDisplay">Defines how the arrow's display should behave when the distance between the start and end points is larger than the arrow size.</param>
            public static void DrawArrow(Color color, Vector3 startPoint, Vector3 endPoint, Vector3 normal, float arrowSize, DownsizeDisplay downsizeDisplay = DownsizeDisplay.Squash, UpsizeDisplay upsizeDisplay = UpsizeDisplay.Offset)
            {
                if (arrowSize < Mathf.Epsilon)
                    return;

                float sizeRatio = (endPoint - startPoint).magnitude * arrowSize / HandleUtility.GetHandleSize((endPoint + startPoint) * 0.5f);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;
                Matrix4x4 baseMatrix = Handles.matrix;

                Matrix4x4 bottomMatrix = Matrix4x4.identity;
                Matrix4x4 topMatrix = Matrix4x4.identity;
                Quaternion matrixRotation = Quaternion.LookRotation(endPoint - startPoint, normal);

                float distance = (endPoint - startPoint).magnitude;

                if (distance > arrowSize)
                {
                    switch (upsizeDisplay)
                    {
                        case UpsizeDisplay.Offset:
                            bottomMatrix = Matrix4x4.TRS(startPoint, matrixRotation, new Vector3(arrowSize, arrowSize, (distance - arrowSize * 0.5f) * 2));
                            topMatrix = Matrix4x4.TRS(endPoint, matrixRotation, Vector3.one * arrowSize);
                            break;

                        case UpsizeDisplay.Stretch:
                            bottomMatrix = Matrix4x4.TRS(startPoint, matrixRotation, new Vector3(arrowSize, arrowSize, distance));
                            topMatrix = Matrix4x4.TRS(endPoint, matrixRotation, new Vector3(arrowSize, arrowSize, distance));
                            break;
                    }
                }
                else
                {
                    switch (downsizeDisplay)
                    {
                        case DownsizeDisplay.Squash:
                            bottomMatrix = Matrix4x4.TRS(startPoint, matrixRotation, new Vector3(arrowSize, arrowSize, distance));
                            topMatrix = Matrix4x4.TRS(endPoint, matrixRotation, new Vector3(arrowSize, arrowSize, distance));
                            break;

                        case DownsizeDisplay.Scale:
                            bottomMatrix = Matrix4x4.TRS(startPoint, matrixRotation, Vector3.one * distance);
                            topMatrix = Matrix4x4.TRS(endPoint, matrixRotation, Vector3.one * distance);
                            break;
                    }
                }

                // Draw
                Handles.matrix = bottomMatrix;

                Handles.color = color;
                Handles.DrawAAPolyLine(VerticesTables.ArrowBottomVertices);
                Handles.color = MultiplyColorAlpha(color, convexShapeColorMultiplier);
                Handles.DrawAAConvexPolygon(VerticesTables.ArrowBottomVertices);

                Handles.matrix = topMatrix;

                Handles.color = color;
                Handles.DrawAAPolyLine(VerticesTables.ArrowTopVertices);
                Handles.color = MultiplyColorAlpha(color, convexShapeColorMultiplier);
                Handles.DrawAAConvexPolygon(VerticesTables.ArrowTopVertices);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
                Handles.matrix = baseMatrix;
            }

            /// <summary>
            /// Draws an arrow always facing the current view.
            /// </summary>
            /// <param name="startPoint">Start point of the arrow.</param>
            /// <param name="endPoint">End point of the arrow.</param>
            /// <param name="arrowSize">Size of the arrow.</param>
            /// <param name="color">Color of the arrow.</param>
            /// <param name="selectable">Defines if the arrow is selectable or not.</param>
            /// <param name="downsizeDisplay">Defines how the arrow's display should behave when the distance between the start and end points is shorter than the arrow size.</param>
            /// <param name="upsizeDisplay">Defines how the arrow's display should behave when the distance between the start and end points is larger than the arrow size.</param>
            public static void DrawViewFacingArrow(Color color, Vector3 startPoint, Vector3 endPoint, float arrowSize, DownsizeDisplay downsizeDisplay = DownsizeDisplay.Squash, UpsizeDisplay upsizeDisplay = UpsizeDisplay.Offset)
            {
                if (arrowSize < Mathf.Epsilon)
                    return;

                if (!Camera.current)
                    return;

                Vector3 direction = (startPoint + endPoint) * 0.5f - Camera.current.transform.position;
                DrawArrow(color, startPoint, endPoint, direction, arrowSize, downsizeDisplay, upsizeDisplay);
            }

            #endregion

            #region JOYSTICK

            /// <summary>
            /// Draws a simple joystick axis visualizer always facing up.
            /// </summary>
            /// <param name="color">The color of the joystick.</param>
            /// <param name="deadZoneColor">The color of the joystick when the axis input is within the dead zone.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="position">The center position of the joystick.</param>
            /// <param name="joystickAxis">The axis input you want to display.</param>
            /// <param name="deadZoneThreshold">A normalized value defining when the axis input is within the dead zone or not.</param>
            /// <param name="forwardAngle">An angle offset to rotate the reference forward vector of the joystick visualizer.</param>
            public static void DrawJoystickAxis(Color color, Color deadZoneColor, float displaySize, Vector3 position, Vector2 joystickAxis, float deadZoneThreshold, float forwardAngle = 0f)
            {
                DrawJoystickAxis(color, deadZoneColor, displaySize, position, joystickAxis, deadZoneThreshold, Quaternion.Euler(0, forwardAngle, 0));
            }

            /// <summary>
            /// Draws a joystick axis visualizer always facing the given normal.
            /// </summary>
            /// <param name="color">The color of the joystick.</param>
            /// <param name="deadZoneColor">The color of the joystick when the axis input is within the dead zone.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="position">The center position of the joystick.</param>
            /// <param name="joystickAxis">The axis input you want to display.</param>
            /// <param name="deadZoneThreshold">A normalized value defining when the axis input is within the dead zone or not.</param>
            /// <param name="normal">The facing normal of the joystick visualizer.</param>
            /// <param name="forward">The reference forward vector of the joystick visualizer.</param>
            public static void DrawJoystickAxis(Color color, Color deadZoneColor, float displaySize, Vector3 position, Vector2 joystickAxis, float deadZoneThreshold, Vector3 normal, Vector3 forward)
            {
                DrawJoystickAxis(color, deadZoneColor, displaySize, position, joystickAxis, deadZoneThreshold, Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, normal), normal));
            }

            /// <summary>
            /// Draws a joystick axis visualizer at the given transform position, using its rotation.
            /// </summary>
            /// <param name="color">The color of the joystick.</param>
            /// <param name="deadZoneColor">The color of the joystick when the axis input is within the dead zone.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="joystickAxis">The axis input you want to display.</param>
            /// <param name="deadZoneThreshold">A normalized value defining when the axis input is within the dead zone or not.</param>
            /// <param name="transform">The transform defining the position and the rotation of the joystick visualizer.</param>
            public static void DrawJoystickAxis(Color color, Color deadZoneColor, float displaySize, Vector2 joystickAxis, float deadZoneThreshold, Transform transform)
            {
                if (!transform)
                    return;

                DrawJoystickAxis(color, deadZoneColor, displaySize, transform.position, joystickAxis, deadZoneThreshold, transform.rotation);
            }

            /// <summary>
            /// Draws a joystick axis visualizer with a given rotation.
            /// </summary>
            /// <param name="color">The color of the joystick.</param>
            /// <param name="deadZoneColor">The color of the joystick when the axis input is within the dead zone.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="position">The center position of the joystick.</param>
            /// <param name="joystickAxis">The axis input you want to display.</param>
            /// <param name="deadZoneThreshold">A normalized value defining when the axis input is within the dead zone or not.</param>
            /// <param name="rotation">The rotation of the joystick visualizer.</param>
            public static void DrawJoystickAxis(Color color, Color deadZoneColor, float displaySize, Vector3 position, Vector2 joystickAxis, float deadZoneThreshold, Quaternion rotation)
            {
                float sizeRatio = displaySize / HandleUtility.GetHandleSize(position);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;
                Matrix4x4 baseMatrix = Handles.matrix;
                float axisMagnitude = Mathf.Clamp(joystickAxis.magnitude, 0f, 1f);
                deadZoneThreshold = Mathf.Clamp(deadZoneThreshold, 0f, 1f);
                bool deadZone = axisMagnitude < deadZoneThreshold;
                Color targetColor = deadZone ? deadZoneColor : color;

                // Draw
                Handles.matrix = Matrix4x4.TRS(position, rotation, Vector3.one * displaySize);
            
                Handles.color = targetColor;
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, 1);

                Handles.DrawAAPolyLine(Quaternion.Euler(0, -8, 0) * Vector3.forward, Vector3.forward * 1.2f, Quaternion.Euler(0, 8, 0) * Vector3.forward);

                Handles.color = MultiplyColorAlpha(targetColor, 0.5f);
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, axisMagnitude);
                Handles.DrawDottedLine(Vector3.back, Vector3.forward, 3);
                Handles.DrawDottedLine(Vector3.left, Vector3.right, 3);

                Quaternion arrowRotation = Quaternion.Euler(0, -Vector2.SignedAngle(Vector2.up, joystickAxis), 0);
                DrawArrow(targetColor, position, position + rotation * arrowRotation * Vector3.forward * displaySize * axisMagnitude, rotation * Vector3.up, displaySize * 0.5f, DownsizeDisplay.Scale);

                Handles.color = deadZoneColor;
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, deadZoneThreshold);
                Handles.color = MultiplyColorAlpha(deadZoneColor, 0.1f);
                Handles.DrawSolidDisc(Vector3.zero, Vector3.up, deadZoneThreshold);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
                Handles.matrix = baseMatrix;
            }

            #endregion

            #region BOX DRAW

            /// <summary>
            /// Draws a box.
            /// </summary>
            /// <param name="color">Color of the box.</param>
            /// <param name="center">Center of the box.</param>
            /// <param name="rotation">Euler rotation of the box.</param>
            /// <param name="size">Local size of the box.</param>
            public static void DrawBox(Color color, Vector3 center, Quaternion rotation, Vector3 size)
            {
                if (size.magnitude < Mathf.Epsilon)
                    return;

                float sizeRatio = size.magnitude / HandleUtility.GetHandleSize(center);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;
                Matrix4x4 baseMatrix = Handles.matrix;

                // Draw
                for (int i = 0; i < VerticesTables.CubeVertices.Length; i++)
                {
                    cubePointsCloud[i] = center + rotation * MultiplyVector3(VerticesTables.CubeVertices[i], size);
                }

                TryDrawConvexShape(cubePointsCloud, color, true);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
                Handles.matrix = baseMatrix;
            }

            /// <summary>
            /// Draws a BoxCollider. Note that the display may be inaccurate if the collider's matrix is skewed because of its parent hierarchy.
            /// </summary>
            /// <param name="color">Color of the box.</param>
            /// <param name="boxCollider">The source BoxCollider to draw.</param>
            /// <param name="positionOffset">An offset position to apply to the gizmos.</param>
            public static void DrawBox(Color color, BoxCollider boxCollider, Vector3 positionOffset = default)
            {
                if (!boxCollider)
                    return;

                DrawBox(
                    color,
                    boxCollider.transform.position + boxCollider.transform.rotation * MultiplyVector3(boxCollider.transform.lossyScale, boxCollider.center) + positionOffset,
                    boxCollider.transform.rotation,
                    MultiplyVector3(boxCollider.transform.lossyScale, boxCollider.size));
            }


            /// <summary>
            /// Draws a boxcast visualizer. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="center">Center of the box being the start point of the boxcast.</param>
            /// <param name="halfExtents">"Radius" of the box used to perform the boxcast.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="orientation">Orientation of the box.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the boxcast.</param>
            /// <param name="queryTriggerInteraction">Defines if the boxcast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Boxcast(Color defaultColor, Color hitColor, float displaySize, Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation = default, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Boxcast(defaultColor, hitColor, displaySize, center, halfExtents, direction, out RaycastHit hitInfo, orientation, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a boxcast visualizer from a BoxCollider. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="boxCollider">The BoxCollider used to perform the boxcast.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the boxcast.</param>
            /// <param name="queryTriggerInteraction">Defines if the boxcast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Boxcast(Color defaultColor, Color hitColor, float displaySize, BoxCollider boxCollider, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Boxcast(defaultColor, hitColor, displaySize, boxCollider, direction, out RaycastHit hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a boxcast visualizer and stores the hit information. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="center">Center of the box being the start point of the boxcast.</param>
            /// <param name="halfExtents">"Radius" of the box used to perform the boxcast.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="orientation">Orientation of the box.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the boxcast.</param>
            /// <param name="queryTriggerInteraction">Defines if the boxcast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Boxcast(Color defaultColor, Color hitColor, float displaySize, Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation = default, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                // Cache
                Vector3 boxSize = halfExtents * 2;
                bool hit = Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance, layerMask, queryTriggerInteraction);

                // Draw
                ShapecastVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, center, direction, out Vector3 end, hitInfo, maxDistance);

                DrawBox(targetColor, center, orientation, boxSize);
                DrawBox(targetColor, end, orientation, boxSize);

                if (hit)
                    DrawBox(MultiplyColorAlpha(targetColor, castHitAlphaMultiplier), center + direction.normalized * maxDistance, orientation, boxSize);

                return hit;
            }

            /// <summary>
            /// Draws a boxcast visualizer from a BoxCollider and stores the hit information. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="boxCollider">The BoxCollider used to perform the boxcast.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the boxcast.</param>
            /// <param name="queryTriggerInteraction">Defines if the boxcast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Boxcast(Color defaultColor, Color hitColor, float displaySize, BoxCollider boxCollider, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                Vector3 center = boxCollider.transform.position + boxCollider.transform.rotation * MultiplyVector3(boxCollider.transform.lossyScale, boxCollider.center);
                Vector3 halfExtents = MultiplyVector3(boxCollider.transform.lossyScale, boxCollider.size) * 0.5f;

                return Boxcast(defaultColor, hitColor, displaySize, center, halfExtents, direction, out hitInfo, boxCollider.transform.rotation, maxDistance, layerMask, queryTriggerInteraction);
            }

            #endregion

            #region SPHERE DRAW

            /// <summary>
            /// Draws a sphere at the given position.
            /// </summary>
            /// <param name="color">The color of the sphere.</param>
            /// <param name="position">The position of the sphere's center.</param>
            /// <param name="radius">The radius of the sphere.</param>
            public static void DrawSphere(Color color, Vector3 position, float radius)
            {
                if (radius < Mathf.Epsilon)
                    return;

                float sizeRatio = radius * 2 / HandleUtility.GetHandleSize(position);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                if (!Camera.current)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;

                // Draw
                GetSphereDisc(position, radius, out Vector3 discCenter, out Vector3 discDirection, out float discRadius);

                Handles.color = color;
                Handles.DrawWireDisc(discCenter, discDirection, discRadius);
                Handles.color = MultiplyColorAlpha(color, solidColorMultiplier);
                Handles.DrawSolidDisc(discCenter, discDirection, discRadius);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
            }

            /// <summary>
            /// Draws a SphereCollider. Note that the display may be inaccurate if the collider's matrix is skewed because of its parent hierarchy.
            /// </summary>
            /// <param name="color">Color of the sphere.</param>
            /// <param name="sphereCollider">The source SphereCollider to draw.</param>
            /// <param name="positionOffset">An offset position to apply to the gizmos.</param>
            public static void DrawSphere(Color color, SphereCollider sphereCollider, Vector3 positionOffset = default)
            {
                if (!sphereCollider)
                    return;

                float maxScale = Mathf.Max(
                    Mathf.Abs(sphereCollider.transform.lossyScale.x),
                    Mathf.Abs(sphereCollider.transform.lossyScale.y),
                    Mathf.Abs(sphereCollider.transform.lossyScale.z));

                DrawSphere(
                    color, 
                    sphereCollider.transform.position + sphereCollider.transform.rotation * MultiplyVector3(sphereCollider.transform.lossyScale, sphereCollider.center) + positionOffset,
                    sphereCollider.radius * maxScale);
            }


            /// <summary>
            /// Draws a spherecast visualizer. Returns true if the spherecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the spherecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the spherecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the sphere being the start point of the spherecast.</param>
            /// <param name="radius">Radius of the sphere used to perform the spherecast.</param>
            /// <param name="direction">Direction of the spherecast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the spherecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the spherecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Spherecast(Color defaultColor, Color hitColor, float displaySize, Vector3 origin, float radius, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Spherecast(defaultColor, hitColor, displaySize, origin, radius, direction, out RaycastHit hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a spherecast visualizer from a SphereCollider. Returns true if the spherecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the spherecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the spherecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="sphereCollider">The SphereCollider used to perform the spherecast.</param>
            /// <param name="direction">Direction of the spherecast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the spherecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the spherecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Spherecast(Color defaultColor, Color hitColor, float displaySize, SphereCollider sphereCollider, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Spherecast(defaultColor, hitColor, displaySize, sphereCollider, direction, out RaycastHit hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a spherecast visualizer and stores the hit information. Returns true if the spherecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the spherecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the spherecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the sphere being the start point of the spherecast.</param>
            /// <param name="radius">Radius of the sphere used to perform the spherecast.</param>
            /// <param name="direction">Direction of the spherecast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the spherecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the spherecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Spherecast(Color defaultColor, Color hitColor, float displaySize, Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                // Cache
                bool hit = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);

                // Draw
                ShapecastVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, origin, direction, out Vector3 end, hitInfo, maxDistance);

                DrawSphere(targetColor, origin, radius);
                DrawSphere(targetColor, end, radius);

                if (hit)
                    DrawSphere(MultiplyColorAlpha(targetColor, castHitAlphaMultiplier), origin + direction.normalized * maxDistance, radius);

                return hit;
            }

            /// <summary>
            /// Draws a spherecast visualizer from a SphereCollider and stores the hit information. Returns true if the spherecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the spherecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the spherecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="sphereCollider">The SphereCollider used to perform the spherecast.</param>
            /// <param name="direction">Direction of the spherecast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the spherecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the spherecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Spherecast(Color defaultColor, Color hitColor, float displaySize, SphereCollider sphereCollider, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                Vector3 center = sphereCollider.transform.position + sphereCollider.transform.rotation * MultiplyVector3(sphereCollider.transform.lossyScale, sphereCollider.center);

                float maxScale = Mathf.Max(
                     Mathf.Abs(sphereCollider.transform.lossyScale.x),
                     Mathf.Abs(sphereCollider.transform.lossyScale.y),
                     Mathf.Abs(sphereCollider.transform.lossyScale.z));

                return Spherecast(defaultColor, hitColor, displaySize, center, sphereCollider.radius * maxScale, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            #endregion

            #region CAPSULE DRAW

            /// <summary>
            /// Draws a capsule.
            /// </summary>
            /// <param name="color">Color of the capsule.</param>
            /// <param name="point1">Center of the capsule's "bottom" sphere.</param>
            /// <param name="point2">Center of the capsule's "top" sphere.</param>
            /// <param name="radius">Radius of the capsule.</param>
            public static void DrawCapsule(Color color, Vector3 point1, Vector3 point2, float radius)
            {
                if (radius < Mathf.Epsilon)
                    return;

                float sizeRatio = ((point2 - point1).magnitude + radius * 2) / HandleUtility.GetHandleSize((point1 + point2) * 0.5f);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                if (!Camera.current)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;
                Matrix4x4 baseMatrix = Handles.matrix;
            
                Vector3[] verticesTable = VerticesTables.Circle8Vertices;
                Vector3[] pointsCloud = capsule8PointsCloud;

                float screenSize = HandleUtility.GetHandleSize((point1 + point2) * 0.5f) / (radius * 2);
                if (screenSize < highDefinitionThreshold)
                {
                    verticesTable = VerticesTables.Circle32Vertices;
                    pointsCloud = capsule32PointsCloud;
                }
                else if (screenSize < midDefinitionThreshold)
                {
                    verticesTable = VerticesTables.Circle16Vertices;
                    pointsCloud = capsule16PointsCloud;
                }

                int tableLength = verticesTable.Length;

                // Draw
                GetSphereDisc(point1, radius, out Vector3 startDiscCenter, out Vector3 startDiscDirection, out float startDiscRadius);
                GetSphereDisc(point2, radius, out Vector3 endDiscCenter, out Vector3 endDiscDirection, out float endDiscRadius);

                Quaternion startHemisphereRotation = Quaternion.LookRotation(startDiscDirection);
                Quaternion endHemisphereRotation = Quaternion.LookRotation(endDiscDirection, Vector3.down);

                for (int i = 0; i < tableLength; i++)
                {
                    pointsCloud[i] = startDiscCenter + startHemisphereRotation * verticesTable[i] * startDiscRadius * 2;
                    pointsCloud[i + tableLength] = endDiscCenter + endHemisphereRotation * verticesTable[i] * endDiscRadius * 2;
                }

                TryDrawConvexShape(pointsCloud, color, true);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
                Handles.matrix = baseMatrix;
            }

            /// <summary>
            /// Draws a capsule.
            /// </summary>
            /// <param name="color">Color of the capsule.</param>
            /// <param name="center">Center of the capsule.</param>
            /// <param name="direction">Direction of the capsule.</param>
            /// <param name="height">Height of the capsule, from the very bottom to the very height.</param>
            /// <param name="radius">Radius of the capsule.</param>
            public static void DrawCapsule(Color color, Vector3 center, Vector3 direction, float height, float radius)
            {
                float shortHeight = Mathf.Max(0f, height - radius * 2);
                DrawCapsule(color, center - direction * shortHeight * 0.5f, center + direction * shortHeight * 0.5f, radius);
            }

            /// <summary>
            /// Draws a CapsuleCollider. Note that the display may be inaccurate if the collider's matrix is skewed because of its parent hierarchy.
            /// </summary>
            /// <param name="color">Color of the capsule.</param>
            /// <param name="capsuleCollider">The source CapsuleCollider to draw.</param>
            /// <param name="positionOffset">An offset position to apply to the gizmos.</param>
            public static void DrawCapsule(Color color, CapsuleCollider capsuleCollider, Vector3 positionOffset = default)
            {
                if (!capsuleCollider)
                    return;

                Vector3 direction = Vector3.forward;
                Vector3 lossyScale = capsuleCollider.transform.lossyScale;
                float maxHeightScale = 1f;
                float maxRadiusScale = 1f;
                float absXScale = Mathf.Abs(lossyScale.x);
                float absYScale = Mathf.Abs(lossyScale.y);
                float absZScale = Mathf.Abs(lossyScale.z);

                switch (capsuleCollider.direction)
                {
                    // X
                    case 0:
                        direction = Vector3.right;
                        maxHeightScale = absXScale;
                        maxRadiusScale = Mathf.Max(absYScale, absZScale);
                        break;

                    // Y
                    case 1:
                        direction = Vector3.up;
                        maxHeightScale = absYScale;
                        maxRadiusScale = Mathf.Max(absXScale, absZScale);
                        break;

                    // Z
                    case 2:
                        direction = Vector3.forward;
                        maxHeightScale = absZScale;
                        maxRadiusScale = Mathf.Max(absXScale, absYScale);
                        break;
                }

                DrawCapsule(
                    color,
                    capsuleCollider.transform.position + capsuleCollider.transform.rotation * MultiplyVector3(lossyScale, capsuleCollider.center) + positionOffset,
                    capsuleCollider.transform.rotation * direction,
                    capsuleCollider.height * maxHeightScale,
                    capsuleCollider.radius * maxRadiusScale);
            }


            /// <summary>
            /// Draws a capsulecast visualizer. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="point1">Center of the capsule's "bottom" sphere.</param>
            /// <param name="point1">Center of the capsule's "top" sphere.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the capsulecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Capsulecast(Color defaultColor, Color hitColor, float displaySize, Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Capsulecast(defaultColor, hitColor, displaySize, point1, point2, radius, direction, out RaycastHit hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a capsulecast visualizer and stores the hit information. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="point1">Center of the capsule's "bottom" sphere.</param>
            /// <param name="point1">Center of the capsule's "top" sphere.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the capsulecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Capsulecast(Color defaultColor, Color hitColor, float displaySize, Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                // Cache
                bool hit = Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);

                // Draw
                Vector3 center = (point1 + point2) * 0.5f;
                Vector3 capsuleDirection = (point2 - point1).normalized;
                float height = (point2 - point1).magnitude + radius * 2;

                ShapecastVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, center, direction, out Vector3 end, hitInfo, maxDistance);

                DrawCapsule(targetColor, center, capsuleDirection, height, radius);
                DrawCapsule(targetColor, end, capsuleDirection, height, radius);

                if (hit)
                    DrawCapsule(targetColor, center + direction.normalized * maxDistance, capsuleDirection, height, radius);

                return hit;
            }

            /// <summary>
            /// Draws a capsulecast visualizer. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="center">Center of the capsule.</param>
            /// <param name="capsuleDirection">Direction of the capsule.</param>
            /// <param name="height">Height of the capsule, from the very bottom to the very height.</param>
            /// <param name="radius">Radius of the capsule.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the capsulecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Capsulecast(Color defaultColor, Color hitColor, float displaySize, Vector3 center, Vector3 capsuleDirection, float height, float radius, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Capsulecast(defaultColor, hitColor, displaySize, center, capsuleDirection, height, radius, direction, out RaycastHit hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a capsulecast visualizer and stores the hit information. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="center">Center of the capsule.</param>
            /// <param name="capsuleDirection">Direction of the capsule.</param>
            /// <param name="height">Height of the capsule, from the very bottom to the very height.</param>
            /// <param name="radius">Radius of the capsule.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the capsulecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Capsulecast(Color defaultColor, Color hitColor, float displaySize, Vector3 center, Vector3 capsuleDirection, float height, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                float shortHeight = Mathf.Max(0f, height - radius * 2);
                return Capsulecast(defaultColor, hitColor, displaySize, center - capsuleDirection.normalized * shortHeight * 0.5f, center + capsuleDirection.normalized * shortHeight * 0.5f, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a capsulecast visualizer from a CapsuleCollider. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="capsuleCollider">The CapsuleCollider used to perform the capsulecast.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the capsulecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Capsulecast(Color defaultColor, Color hitColor, float displaySize, CapsuleCollider capsuleCollider, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return Capsulecast(defaultColor, hitColor, displaySize, capsuleCollider, direction, out RaycastHit hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a capsulecast visualizer from a CapsuleCollider and stores the hit information. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="capsuleCollider">The CapsuleCollider used to perform the capsulecast.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the capsulecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Capsulecast(Color defaultColor, Color hitColor, float displaySize, CapsuleCollider capsuleCollider, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                // Cache
                Vector3 capsuleDirection = Vector3.forward;
                Vector3 lossyScale = capsuleCollider.transform.lossyScale;
                float maxHeightScale = 1f;
                float maxRadiusScale = 1f;
                float absXScale = Mathf.Abs(lossyScale.x);
                float absYScale = Mathf.Abs(lossyScale.y);
                float absZScale = Mathf.Abs(lossyScale.z);

                switch (capsuleCollider.direction)
                {
                    // X
                    case 0:
                        capsuleDirection = Vector3.right;
                        maxHeightScale = absXScale;
                        maxRadiusScale = Mathf.Max(absYScale, absZScale);
                        break;

                    // Y
                    case 1:
                        capsuleDirection = Vector3.up;
                        maxHeightScale = absYScale;
                        maxRadiusScale = Mathf.Max(absXScale, absZScale);
                        break;

                    // Z
                    case 2:
                        capsuleDirection = Vector3.forward;
                        maxHeightScale = absZScale;
                        maxRadiusScale = Mathf.Max(absXScale, absYScale);
                        break;
                }

                Vector3 center = capsuleCollider.transform.position + capsuleCollider.transform.rotation * MultiplyVector3(lossyScale, capsuleCollider.center);
                Vector3 heightVector = capsuleCollider.transform.rotation * capsuleDirection * (capsuleCollider.height * maxHeightScale - capsuleCollider.radius * 2 * maxRadiusScale) * 0.5f;

                bool hit = Physics.CapsuleCast(center + heightVector, center - heightVector, capsuleCollider.radius * maxRadiusScale, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);

                // Draw
                ShapecastVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, center, direction, out Vector3 end, hitInfo, maxDistance);

                DrawCapsule(targetColor, capsuleCollider);
                DrawCapsule(targetColor, capsuleCollider, direction.normalized * (end - center).magnitude);

                if (hit)
                    DrawCapsule(MultiplyColorAlpha(targetColor, castHitAlphaMultiplier), capsuleCollider, direction.normalized * maxDistance);

                return hit;
            }

            #endregion

            #region LINECAST DRAW

            /// <summary>
            /// Draws a linecast visualizer. Returns true if the linecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the linecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the linecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="start">The start point of the linecast.</param>
            /// <param name="end">The end point of the linecast.</param>
            /// <param name="layerMask">Defines which layers can be hit with the linecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the linecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Linecast(Color defaultColor, Color hitColor, float displaySize, Vector3 start, Vector3 end, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return LinecastVisualizer(defaultColor, hitColor, displaySize, start, end, out RaycastHit hitInfo, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a linecast visualizer and stores the hit information. Returns true if the linecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the linecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the linecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="start">The start point of the linecast.</param>
            /// <param name="end">The end point of the linecast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="layerMask">Defines which layers can be hit with the linecast.</param>
            /// <param name="queryTriggerInteraction">Defines if the linecast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Linecast(Color defaultColor, Color hitColor, float displaySize, Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return LinecastVisualizer(defaultColor, hitColor, displaySize, start, end, out hitInfo, layerMask, queryTriggerInteraction);
            }

            #endregion

            #region RAYCAST DRAW

            /// <summary>
            /// Draws a raycast visualizer. Returns true if it hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the raycast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the raycast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="ray">The ray used to perform the raycast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the raycast.</param>
            /// <param name="queryTriggerInteraction">Defines if the raycast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Raycast(Color defaultColor, Color hitColor, float displaySize, Ray ray, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return LinecastVisualizer(defaultColor, hitColor, displaySize, ray.origin, ray.origin + ray.direction.normalized * maxDistance, out RaycastHit hitInfo, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a raycast visualizer and stores the hit information. Returns true if the raycast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the raycast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the raycast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="ray">The ray used to perform the raycast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the raycast.</param>
            /// <param name="queryTriggerInteraction">Defines if the raycast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Raycast(Color defaultColor, Color hitColor, float displaySize, Ray ray, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return LinecastVisualizer(defaultColor, hitColor, displaySize, ray.origin, ray.origin + ray.direction.normalized * maxDistance, out hitInfo, layerMask, queryTriggerInteraction);
            }


            /// <summary>
            /// Draws a raycast visualizer. Returns true if it hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the raycast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the raycast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Start point of the raycast.</param>
            /// <param name="direction">Direction of the raycast.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the raycast.</param>
            /// <param name="queryTriggerInteraction">Defines if the raycast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Raycast(Color defaultColor, Color hitColor, float displaySize, Vector3 origin, Vector3 direction, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return LinecastVisualizer(defaultColor, hitColor, displaySize, origin, origin + direction.normalized * maxDistance, out RaycastHit hitInfo, layerMask, queryTriggerInteraction);
            }

            /// <summary>
            /// Draws a raycast visualizer and stores the hit information. Returns true if the raycast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the raycast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the raycast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Start point of the raycast.</param>
            /// <param name="direction">Direction of the raycast.</param>
            /// <param name="hitInfo">Stores the hit information.</param>
            /// <param name="maxDistance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the raycast.</param>
            /// <param name="queryTriggerInteraction">Defines if the raycast can hit triggers or not.</param>
            /// <returns></returns>
            public static bool Raycast(Color defaultColor, Color hitColor, float displaySize, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.MaxValue, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                return LinecastVisualizer(defaultColor, hitColor, displaySize, origin, origin + direction.normalized * maxDistance, out hitInfo, layerMask, queryTriggerInteraction);
            }

            #endregion

            #region LINECAST 2D DRAW

            /// <summary>
            /// Draws a 2D linecast visualizer. Returns true if the linecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the linecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the linecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="start">Start point of the linecast.</param>
            /// <param name="end">End point of the linecast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <returns></returns>
            public static bool Linecast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 start, Vector2 end, ContactFilter2D contactFilter2D)
            {
                return LinecastVisualizer2D(defaultColor, hitColor, displaySize, start, end, contactFilter2D);
            }

            /// <summary>
            /// Draws a 2D linecast visualizer. Returns true if the linecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the linecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the linecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="start">Start point of the linecast.</param>
            /// <param name="end">End point of the linecast.</param>
            /// <param name="layerMask">Defines which layers can be hit with the linecast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Linecast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 start, Vector2 end, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return LinecastVisualizer2D(defaultColor, hitColor, displaySize, start, end, contactFilter2D);
            }

            #endregion

            #region RAYCAST 2D DRAW

            /// <summary>
            /// Draws a 2D raycast visualizer. Returns true if the raycast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the raycast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the raycast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Start point of the raycast.</param>
            /// <param name="direction">Direction of the raycast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Raycast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                return LinecastVisualizer2D(defaultColor, hitColor, displaySize, origin, origin + direction.normalized * distance, contactFilter2D);
            }

            /// <summary>
            /// Draws a 2D raycast visualizer. Returns true if the raycast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the raycast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the raycast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Start point of the raycast.</param>
            /// <param name="direction">Direction of the raycast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the raycast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Raycast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth =  float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return LinecastVisualizer2D(defaultColor, hitColor, displaySize, origin, origin + direction.normalized * distance, contactFilter2D);
            }

            #endregion

            #region BOX 2D DRAW

            /// <summary>
            /// Draws a 2D box.
            /// </summary>
            /// <param name="color">The color of the box.</param>
            /// <param name="center">The center of the box.</param>
            /// <param name="size">The size of the box.</param>
            /// <param name="rotation">The rotation of the box.</param>
            /// <param name="edgeRadius">An offset that can be applied to the edges of the box while rounding the resulting shape around its corners.</param>
            public static void DrawBox2D(Color color, Vector3 center, Vector2 size, Quaternion rotation, float edgeRadius = 0f)
            {
                float sizeRatio = (size.magnitude + edgeRadius * 2) / HandleUtility.GetHandleSize(center);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Cache
                Matrix4x4 boxMatrix = Matrix4x4.TRS(center, rotation, size * 0.5f);

                Vector3 SetZValue(Vector3 point)
                {
                    return new Vector3(point.x, point.y, center.z);
                }

                // Draw
                box2DPointsCloud[0] = box2DPointsCloud[4] = SetZValue(boxMatrix.MultiplyPoint3x4(new Vector3(-1, -1)));
                box2DPointsCloud[1] = SetZValue(boxMatrix.MultiplyPoint3x4(new Vector3(-1, 1)));
                box2DPointsCloud[2] = SetZValue(boxMatrix.MultiplyPoint3x4(new Vector3(1, 1)));
                box2DPointsCloud[3] = SetZValue(boxMatrix.MultiplyPoint3x4(new Vector3(1, -1)));

                if (edgeRadius > 0f) // Rouded
                {
                    Vector3[] verticesTable = VerticesTables.Circle8Vertices;
                    Vector3[] pointsCloud = box2DRounded8PointsCloud;

                    float screenSize = HandleUtility.GetHandleSize(center) / (edgeRadius * 2);
                    if (screenSize < highDefinitionThreshold)
                    {
                        verticesTable = VerticesTables.Circle32Vertices;
                        pointsCloud = box2DRounded32PointsCloud;
                    }
                    else if (screenSize < midDefinitionThreshold)
                    {
                        verticesTable = VerticesTables.Circle16Vertices;
                        pointsCloud = box2DRounded16PointsCloud;
                    }

                    int tableLength = verticesTable.Length;

                    for (int i = 0; i < tableLength; i++)
                    {
                        Vector3 circlePoint = verticesTable[i] * edgeRadius * 2;
                    
                        pointsCloud[i] = box2DPointsCloud[0] + circlePoint;
                        pointsCloud[i + tableLength] = box2DPointsCloud[1] + circlePoint;
                        pointsCloud[i + tableLength * 2] = box2DPointsCloud[2] + circlePoint;
                        pointsCloud[i + tableLength * 3] = box2DPointsCloud[3] + circlePoint;
                    }

                    TryDrawConvexShape(pointsCloud, color, true);
                }
                else
                {
                    TryDrawConvexShape(box2DPointsCloud, color, false);
                }
            }

            /// <summary>
            /// Draws a 2D box.
            /// </summary>
            /// <param name="color">The color of the box.</param>
            /// <param name="center">The center of the box.</param>
            /// <param name="size">The size of the box.</param>
            /// <param name="angle">Z rotation of the box in degrees.</param>
            /// <param name="edgeRadius">An offset that can be applied to the edges of the box while rounding the resulting shape around its corners.</param>
            public static void DrawBox2D(Color color, Vector3 center, Vector2 size, float angle, float edgeRadius = 0f)
            {
                DrawBox2D(color, center, size, Quaternion.Euler(0, 0, angle), edgeRadius);
            }

            /// <summary>
            /// Draws a BoxCollider2D. Note that the display may be inaccurate if the collider's matrix is skewed because of its parent hierarchy.
            /// </summary>
            /// <param name="color">The color of the box.</param>
            /// <param name="boxCollider2D">The source BoxCollider2D to draw.</param>
            /// <param name="positionOffset">An offset position to apply to the gizmos.</param>
            public static void DrawBox2D(Color color, BoxCollider2D boxCollider2D, Vector3 positionOffset = default)
            {
                Vector3 position = boxCollider2D.transform.position + boxCollider2D.transform.rotation * MultiplyVector3(boxCollider2D.offset, boxCollider2D.transform.lossyScale) + positionOffset;
                DrawBox2D(color, position, MultiplyVector3(boxCollider2D.transform.lossyScale, boxCollider2D.size), boxCollider2D.transform.rotation, boxCollider2D.edgeRadius);
            }


            /// <summary>
            /// Draws a boxcast visualizer. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the box, being the start point of the boxcast.</param>
            /// <param name="size">Size of the box used to perform the boxcast.</param>
            /// <param name="angle">Z rotation of the box in degrees.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Boxcast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                // Cache
                bool queriesStartInColliders = Physics2D.queriesStartInColliders;
                Physics2D.queriesStartInColliders = false;

                RaycastHit2D raycastHit2D = Physics2D.BoxCast(origin, size, angle, direction, distance, contactFilter2D.layerMask, contactFilter2D.minDepth, contactFilter2D.maxDepth);

                // Draw
                Shapecast2DVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, origin, direction, out Vector3 end, raycastHit2D, distance);

                DrawBox2D(targetColor, origin, size, Quaternion.Euler(0, 0, angle));
                DrawBox2D(targetColor, end, size, Quaternion.Euler(0, 0, angle));

                if (raycastHit2D.collider)
                    DrawBox2D(MultiplyColorAlpha(targetColor, castHitAlphaMultiplier), origin + direction.normalized * distance, size, Quaternion.Euler(0, 0, angle));

                Physics2D.queriesStartInColliders = queriesStartInColliders;

                return raycastHit2D.collider;
            }

            /// <summary>
            /// Draws a boxcast visualizer from a BoxCollider2D. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="boxCollider2D">The source BoxCollider2D to draw.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Boxcast2D(Color defaultColor, Color hitColor, float displaySize, BoxCollider2D boxCollider2D, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                Vector2 center = boxCollider2D.transform.position + boxCollider2D.transform.rotation * MultiplyVector3(boxCollider2D.transform.lossyScale, boxCollider2D.offset);
                Vector2 size = boxCollider2D.transform.lossyScale * boxCollider2D.size;
                return Boxcast2D(defaultColor, hitColor, displaySize, center, size, boxCollider2D.transform.rotation.eulerAngles.z, direction, contactFilter2D, distance);
            }

            /// <summary>
            /// Draws a boxcast visualizer. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the box being the start point of the boxcast.</param>
            /// <param name="size">Size of the box used to perform the boxcast.</param>
            /// <param name="angle">Z rotation of the box in degrees.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the boxcast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Boxcast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return Boxcast2D(defaultColor, hitColor, displaySize, origin, size, angle, direction, contactFilter2D, distance);
            }

            /// <summary>
            /// Draws a boxcast visualizer from a BoxCollider2D. Returns true if the boxcast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the boxcast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the boxcast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="boxCollider2D">The source BoxCollider2D to draw.</param>
            /// <param name="direction">Direction of the boxcast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the boxcast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Boxcast2D(Color defaultColor, Color hitColor, float displaySize, BoxCollider2D boxCollider2D, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return Boxcast2D(defaultColor, hitColor, displaySize, boxCollider2D, direction, contactFilter2D, distance);
            }

            #endregion

            #region CIRCLE 2D DRAW

            /// <summary>
            /// Draws a 2D circle.
            /// </summary>
            /// <param name="color">The color of the circle.</param>
            /// <param name="position">The center of the circle.</param>
            /// <param name="normal">The normal of the circle.</param>
            /// <param name="radius">The radius of the circle.</param>
            public static void DrawCircle2D(Color color, Vector3 position, Vector3 normal, float radius)
            {
                if (radius < 0f)
                    return;

                float sizeRatio = radius * 2 / HandleUtility.GetHandleSize(position);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;

                // Draw
                Handles.color = color;
                Handles.DrawWireDisc(position, normal, radius);
                Handles.color = MultiplyColorAlpha(color, solidColorMultiplier);
                Handles.DrawSolidDisc(position, normal, radius);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
            }

            /// <summary>
            /// Draws a CircleCollider. Note that the display may be inaccurate if the collider's matrix is skewed because of its parent hierarchy.
            /// </summary>
            /// <param name="color">Color of the circle.</param>
            /// <param name="circleCollider2D">The source CircleCollider2D to draw.</param>
            /// <param name="positionOffset">An offset position to apply to the gizmos.</param>
            public static void DrawCircle2D(Color color, CircleCollider2D circleCollider2D, Vector2 positionOffset = default)
            {
                float maxScale = Mathf.Max(Mathf.Abs(circleCollider2D.transform.lossyScale.x), Mathf.Abs(circleCollider2D.transform.lossyScale.y));
                Vector2 position = circleCollider2D.transform.position + circleCollider2D.transform.rotation * MultiplyVector3(circleCollider2D.offset, circleCollider2D.transform.lossyScale);
            
                DrawCircle2D(color, position + positionOffset, Vector3.forward, circleCollider2D.radius * maxScale);
            }


            /// <summary>
            /// Draws a circlecast visualizer. Returns true if the circlecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the circlecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the circlecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the circle, being the start point of the circlecast.</param>
            /// <param name="radius">Radius of the circle used to perform the circlecast.</param>
            /// <param name="direction">Direction of the circlecast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Circlecast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                // Cache
                bool queriesStartInColliders = Physics2D.queriesStartInColliders;
                Physics2D.queriesStartInColliders = false;

                RaycastHit2D raycastHit2D = Physics2D.CircleCast(origin, radius, direction, distance, contactFilter2D.layerMask, contactFilter2D.minDepth, contactFilter2D.maxDepth);

                // Draw
                Shapecast2DVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, origin, direction, out Vector3 end, raycastHit2D, distance);

                DrawCircle2D(targetColor, origin, Vector3.forward, radius);
                DrawCircle2D(targetColor, end, Vector3.forward, radius);

                if (raycastHit2D.collider)
                    DrawCircle2D(MultiplyColorAlpha(targetColor, castHitAlphaMultiplier), origin + direction.normalized * distance, Vector3.forward, radius);

                Physics2D.queriesStartInColliders = queriesStartInColliders;

                return raycastHit2D.collider;
            }

            /// <summary>
            /// Draws a circlecast visualizer from a CircleCollider2D. Returns true if the circlecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the circlecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the circlecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="circleCollider2D">The source CircleCollider2D to draw.</param>
            /// <param name="direction">Direction of the circlecast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Circlecast2D(Color defaultColor, Color hitColor, float displaySize, CircleCollider2D circleCollider2D, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                Vector2 center = circleCollider2D.transform.position + circleCollider2D.transform.rotation * MultiplyVector3(circleCollider2D.transform.lossyScale, circleCollider2D.offset);

                float maxScale = Mathf.Max(
                    Mathf.Abs(circleCollider2D.transform.lossyScale.x),
                    Mathf.Abs(circleCollider2D.transform.lossyScale.y));

                return Circlecast2D(defaultColor, hitColor, displaySize, center, circleCollider2D.radius * maxScale, direction, contactFilter2D, distance);
            }

            /// <summary>
            /// Draws a circlecast visualizer. Returns true if the circlecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the circlecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the circlecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the circle being the start point of the circlecast.</param>
            /// <param name="radius">Radius of the circle used to perform the circlecast.</param>
            /// <param name="direction">Direction of the circlecast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the circlecast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Circlecast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, float radius, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return Circlecast2D(defaultColor, hitColor, displaySize, origin, radius, direction, contactFilter2D, distance);
            }

            /// <summary>
            /// Draws a circlecast visualizer from a CircleCollider2D. Returns true if the circlecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the circlecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the circlecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="circleCollider2D">The source CircleCollider2D to draw.</param>
            /// <param name="direction">Direction of the circlecast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the circlecast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Circlecast2D(Color defaultColor, Color hitColor, float displaySize, CircleCollider2D circleCollider2D, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return Circlecast2D(defaultColor, hitColor, displaySize, circleCollider2D, direction, contactFilter2D, distance);
            }

            #endregion

            #region CAPSULE 2D DRAW

            /// <summary>
            /// Draws a 2D capsule.
            /// </summary>
            /// <param name="color">The color of the capsule.</param>
            /// <param name="center">The center of the capsule.</param>
            /// <param name="size">The size of the capsule.</param>
            /// <param name="rotation">The rotation of the capsule.</param>
            /// <param name="capsuleDirection">The direction of the capsule.</param>
            public static void DrawCapsule2D(Color color, Vector3 center, Vector2 size, Quaternion rotation, CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical)
            {
                if (size.x * size.x < Mathf.Epsilon * Mathf.Epsilon || size.y * size.y < Mathf.Epsilon * Mathf.Epsilon)
                    return;

                float sizeRatio = size.magnitude / HandleUtility.GetHandleSize(center);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Cache
                Color baseHandlesColor = Handles.color;
                Matrix4x4 baseHandlesMatrix = Handles.matrix;
                Matrix4x4 capsuleMatrix = Matrix4x4.TRS(center, rotation, size * 0.5f);

                Vector3 SetZValue(Vector3 point)
                {
                    return new Vector3(point.x, point.y, center.z);
                }

                Vector3 bottom = SetZValue(capsuleMatrix.MultiplyPoint3x4(Vector3.down));
                Vector3 top = SetZValue(capsuleMatrix.MultiplyPoint3x4(Vector3.up));
                Vector3 left = SetZValue(capsuleMatrix.MultiplyPoint3x4(Vector3.left));
                Vector3 right = SetZValue(capsuleMatrix.MultiplyPoint3x4(Vector3.right));

                float radius = (capsuleDirection == CapsuleDirection2D.Vertical ? right - left : top - bottom).magnitude * 0.5f;
                Vector3 offset = capsuleDirection == CapsuleDirection2D.Vertical ? top - bottom : right - left;
                bool circle = offset.magnitude < radius * 2;
                float height = Mathf.Max(offset.magnitude, radius * 2);
                float shortHeight = height * 0.5f - radius;

                // Draw
                Handles.matrix = Matrix4x4.TRS(center, Quaternion.FromToRotation(Vector3.up, offset), Vector2.one);

                Handles.color = color;
                Handles.DrawWireArc(Vector3.down * shortHeight, Vector3.forward, Vector3.left, 180, radius);
                Handles.DrawWireArc(Vector3.up * shortHeight, Vector3.forward, Vector3.right, 180, radius);
                if (!circle)
                {
                    Handles.DrawAAPolyLine(new Vector2(radius, -shortHeight), new Vector2(radius, shortHeight));
                    Handles.DrawAAPolyLine(new Vector2(-radius, -shortHeight), new Vector2(-radius, shortHeight));
                }

                Handles.color = MultiplyColorAlpha(color, solidColorMultiplier);
                Handles.DrawSolidArc(Vector3.down * shortHeight, Vector3.forward, Vector3.left, 180, radius);
                Handles.DrawSolidArc(Vector3.up * shortHeight, Vector3.forward, Vector3.right, 180, radius);
                if (!circle)
                {
                    Handles.color = MultiplyColorAlpha(color, convexShapeColorMultiplier);
                    Handles.DrawAAConvexPolygon(new Vector2(radius, -shortHeight), new Vector2(radius, shortHeight), new Vector2(-radius, shortHeight), new Vector2(-radius, -shortHeight));
                }

                // Revert handles parameters
                Handles.color = baseHandlesColor;
                Handles.matrix = baseHandlesMatrix;
            }

            /// <summary>
            /// Draws a 2D capsule.
            /// </summary>
            /// <param name="color">The color of the capsule.</param>
            /// <param name="center">The center of the capsule.</param>
            /// <param name="size">The size of the capsule.</param>
            /// <param name="angle">Z rotation of the capsule in degrees.</param>
            /// <param name="capsuleDirection">The direction of the capsule.</param>
            public static void DrawCapsule2D(Color color, Vector3 center, Vector2 size, float angle, CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical)
            {
                DrawCapsule2D(color, center, size, Quaternion.Euler(0, 0, angle), capsuleDirection);
            }

            /// <summary>
            /// Draws a CapsuleCollider2D. Note that the display may be inaccurate if the collider's matrix is skewed because of its parent hierarchy.
            /// </summary>
            /// <param name="color">The color of the capsule.</param>
            /// <param name="capsuleCollider2D">The source CapsuleCollider2D to draw.</param>
            /// <param name="positionOffset">An offset position to apply to the gizmos.</param>
            public static void DrawCapsule2D(Color color, CapsuleCollider2D capsuleCollider2D, Vector3 positionOffset = default)
            {
                Vector3 position = capsuleCollider2D.transform.position + capsuleCollider2D.transform.rotation * MultiplyVector3(capsuleCollider2D.offset, capsuleCollider2D.transform.lossyScale) + positionOffset;
                DrawCapsule2D(color, position, MultiplyVector3(capsuleCollider2D.transform.lossyScale, capsuleCollider2D.size), capsuleCollider2D.transform.rotation, capsuleCollider2D.direction);
            }


            /// <summary>
            /// Draws a capsulecast visualizer. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the capsule, being the start point of the capsulecast.</param>
            /// <param name="size">Size of the capsule used to perform the capsulecast.</param>
            /// <param name="capsuleDirection">The direction of the capsule used to perform the capsulecast.</param>
            /// <param name="angle">Z rotation of the capsule in degrees.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Capsulecast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                // Cache
                bool queriesStartInColliders = Physics2D.queriesStartInColliders;
                Physics2D.queriesStartInColliders = false;

                RaycastHit2D raycastHit2D = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, contactFilter2D.layerMask, contactFilter2D.minDepth, contactFilter2D.maxDepth);

                // Draw
                Shapecast2DVisualizer(defaultColor, hitColor, out Color targetColor, displaySize, origin, direction, out Vector3 end, raycastHit2D, distance);

                DrawCapsule2D(targetColor, origin, size, angle, capsuleDirection);
                DrawCapsule2D(targetColor, end, size, angle, capsuleDirection);

                if (raycastHit2D.collider)
                    DrawCapsule2D(MultiplyColorAlpha(targetColor, castHitAlphaMultiplier), origin + direction.normalized * distance, size, angle, capsuleDirection);

                Physics2D.queriesStartInColliders = queriesStartInColliders;

                return raycastHit2D.collider;
            }

            /// <summary>
            /// Draws a capsulecast visualizer from a CapsuleCollider2D. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="capsuleCollider2D">The source CapsuleCollider2D to draw.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="contactFilter2D">The contact filter used to filter the results differently, such as by layer mask, Z depth, or normal angle.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <returns></returns>
            public static bool Capsulecast2D(Color defaultColor, Color hitColor, float displaySize, CapsuleCollider2D capsuleCollider2D, Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.MaxValue)
            {
                Vector2 center = capsuleCollider2D.transform.position + capsuleCollider2D.transform.rotation * MultiplyVector3(capsuleCollider2D.transform.lossyScale, capsuleCollider2D.offset);
                Vector2 size = capsuleCollider2D.transform.lossyScale * capsuleCollider2D.size;
                return Capsulecast2D(defaultColor, hitColor, displaySize, center, size, capsuleCollider2D.direction, capsuleCollider2D.transform.rotation.eulerAngles.z, direction, contactFilter2D, distance);
            }

            /// <summary>
            /// Draws a capsulecast visualizer. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="origin">Center of the capsule, being the start point of the capsulecast.</param>
            /// <param name="size">Size of the capsule used to perform the capsulecast.</param>
            /// <param name="capsuleDirection">The direction of the capsule used to perform the capsulecast.</param>
            /// <param name="angle">Z rotation of the capsule in degrees.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Capsulecast2D(Color defaultColor, Color hitColor, float displaySize, Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return Capsulecast2D(defaultColor, hitColor, displaySize, origin, size, capsuleDirection, angle, direction, contactFilter2D, distance);
            }

            /// <summary>
            /// Draws a capsulecast visualizer from a CapsuleCollider2D. Returns true if the capsulecast hits something.
            /// </summary>
            /// <param name="defaultColor">Used color when the capsulecast doesn't hit anything.</param>
            /// <param name="hitColor">Used color when the capsulecast hits something.</param>
            /// <param name="displaySize">An arbitrary size used to draw the different visualizer elements.</param>
            /// <param name="capsuleCollider2D">The source CapsuleCollider2D to draw.</param>
            /// <param name="direction">Direction of the capsulecast.</param>
            /// <param name="distance">Maximum cast distance.</param>
            /// <param name="layerMask">Defines which layers can be hit with the capsulecast.</param>
            /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
            /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
            /// <returns></returns>
            public static bool Capsulecast2D(Color defaultColor, Color hitColor, float displaySize, CapsuleCollider2D capsuleCollider2D, Vector2 direction, float distance = float.MaxValue, int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D() { layerMask = layerMask, minDepth = minDepth, maxDepth = maxDepth };
                return Capsulecast2D(defaultColor, hitColor, displaySize, capsuleCollider2D, direction, contactFilter2D, distance);
            }

            #endregion

            #region PRIVATE UTILS

            private static void UpdateAnimationParameters()
            {
                timeWaveScaleMultiplier = 1f + Mathf.Cos(Time.realtimeSinceStartup * 8) * timeWaveScaleAmplitude / 2f;
            }

            private static bool LinecastVisualizer(Color defaultColor, Color hitColor, float displaySize, Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
            {
                float sizeRatio = (end - start).magnitude * displaySize / HandleUtility.GetHandleSize((end + start) * 0.5f);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return Physics.Linecast(start, end, out hitInfo, layerMask, queryTriggerInteraction);

                // Cache
                bool hit;
                Color baseHandlesColor = Handles.color;
                Vector3 hitNormal = start - end;
                Color targetColor = defaultColor;
                float sphereHandlesSize = displaySize * 0.1f;

                // Draw
                if (Physics.Linecast(start, end, out hitInfo, layerMask, queryTriggerInteraction))
                {
                    hit = true;
                    hitNormal = hitInfo.normal;
                    targetColor = hitColor;

                    Handles.color = targetColor;
                    Handles.SphereHandleCap(-1, end, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                    Handles.DrawDottedLine(hitInfo.point, end, 3);

                    Handles.color = MultiplyColorAlpha(targetColor, castHitAlphaMultiplier);
                    DrawViewFacingArrow(hitColor, hitInfo.point, hitInfo.point + hitInfo.normal * displaySize * 0.5f, displaySize * 0.25f);

                    end = hitInfo.point;
                }
                else
                {
                    hit = false;
                }

                Handles.color = targetColor;
                Handles.SphereHandleCap(-1, start, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                Handles.SphereHandleCap(-1, end, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                Handles.DrawAAPolyLine(start, end);

                DrawAnimatedCircle(hit ? targetColor : MultiplyColorAlpha(targetColor, 0.25f), end, hitNormal, displaySize * 0.25f, hit);

                // Revert handles parameters
                Handles.color = baseHandlesColor;

                return hit;
            }

            private static void ShapecastVisualizer(Color defaultColor, Color hitColor, out Color targetColor, float displaySize, Vector3 start, Vector3 direction, out Vector3 end, RaycastHit hitInfo, float maxDistance)
            {
                ShapecastVisualizer(defaultColor, hitColor, out targetColor, displaySize, start, direction, out end, maxDistance, hitInfo.collider, hitInfo.distance, hitInfo.normal, hitInfo.point);
            }

            private static void Shapecast2DVisualizer(Color defaultColor, Color hitColor, out Color targetColor, float displaySize, Vector2 start, Vector2 direction, out Vector3 end, RaycastHit2D hitInfo, float maxDistance)
            {
                ShapecastVisualizer(defaultColor, hitColor, out targetColor, displaySize, start, direction, out end, maxDistance, hitInfo.collider, hitInfo.distance, hitInfo.normal, hitInfo.point);
            }

            private static void ShapecastVisualizer(Color defaultColor, Color hitColor, out Color targetColor, float displaySize, Vector3 start, Vector3 direction, out Vector3 end, float maxDistance, bool hit, float hitDistance, Vector3 hitNormal, Vector3 hitPoint)
            {
                // Cache
                Color baseHandlesColor = Handles.color;
                float sphereHandlesSize = Mathf.Max(displaySize * 0.1f, 0f);
                float arrowSize = displaySize * 0.5f;
                targetColor = hit ? hitColor : defaultColor;
                end = start + direction.normalized * (hit ? hitDistance : maxDistance);

                float sizeRatio = maxDistance * displaySize / HandleUtility.GetHandleSize(start + direction.normalized * maxDistance * 0.5f);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return;

                // Draw
                if (hit)
                {
                    Vector3 stepPos = start + direction.normalized * hitDistance;
                    Vector3 hitShapeCenter = start + direction.normalized * maxDistance;

                    Handles.color = targetColor;
                    Handles.SphereHandleCap(-1, hitShapeCenter, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                    Handles.DrawDottedLine(stepPos, hitShapeCenter, 3);

                    Handles.SphereHandleCap(-1, hitPoint, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                    DrawAnimatedCircle(targetColor, hitPoint, hitNormal, displaySize * 0.25f);

                    Handles.color = MultiplyColorAlpha(targetColor, castHitAlphaMultiplier);
                    DrawViewFacingArrow(hitColor, hitPoint, hitPoint + hitNormal * displaySize * 0.5f, displaySize * 0.25f);
                }

                Handles.color = targetColor;
                Handles.SphereHandleCap(-1, start, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                Handles.SphereHandleCap(-1, end, Quaternion.identity, sphereHandlesSize, EventType.Repaint);

                if (displaySize > 0f)
                    DrawViewFacingArrow(targetColor, start, end, arrowSize);
                else
                    Handles.DrawAAPolyLine(start, end);

                // Revert handles parameters
                Handles.color = baseHandlesColor;
            }

            private static bool LinecastVisualizer2D(Color defaultColor, Color hitColor, float displaySize, Vector2 start, Vector2 end, ContactFilter2D contactFilter)
            {
                float sizeRatio = (end - start).magnitude * displaySize / HandleUtility.GetHandleSize((end + start) * 0.5f);
                if (sizeRatio < sizeRatioDiscardThreshold)
                    return Physics2D.Linecast(start, end, contactFilter, cast2DResults) > 0;

                // Cache
                bool hit;
                Color baseHandlesColor = Handles.color;
                Vector2 hitNormal = start - end;
                Color targetColor = defaultColor;
                float sphereHandlesSize = displaySize * 0.1f;
                bool queriesStartInColliders = Physics2D.queriesStartInColliders;
                Physics2D.queriesStartInColliders = false;

                // Draw
                if (Physics2D.Linecast(start, end, contactFilter, cast2DResults) > 0)
                {
                    hit = true;
                    hitNormal = cast2DResults[0].normal;
                    targetColor = hitColor;

                    Handles.color = targetColor;
                    Handles.SphereHandleCap(-1, end, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                    Handles.DrawDottedLine(cast2DResults[0].point, end, 3);

                    Handles.color = MultiplyColorAlpha(targetColor, castHitAlphaMultiplier);
                    DrawArrow(hitColor, cast2DResults[0].point, cast2DResults[0].point + hitNormal * displaySize * 0.5f, Vector3.forward, displaySize * 0.25f);

                    end = cast2DResults[0].point;
                }
                else
                {
                    hit = false;
                }

                Handles.color = targetColor;
                Handles.SphereHandleCap(-1, start, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                Handles.SphereHandleCap(-1, end, Quaternion.identity, sphereHandlesSize, EventType.Repaint);
                Handles.DrawAAPolyLine(start, end);

                DrawAnimatedCircle(hit ? targetColor : MultiplyColorAlpha(targetColor, 0.25f), end, hitNormal, displaySize * 0.25f, hit);

                // Revert handles and physics parameters
                Handles.color = baseHandlesColor;
                Physics2D.queriesStartInColliders = queriesStartInColliders;

                return hit;
            }

            private static Color MultiplyColorAlpha(Color color, float alpha)
            {
                return new Color(color.r, color.g, color.b, color.a * alpha);
            }

            private static bool GetConvexShape(Vector3[] pointsCloud)
            {
                if (!Camera.current)
                    return false;

                // Cache
                int pointsCount = pointsCloud.Length;

                if (pointsCount <= 0)
                    return false;

                // Get leftmost point
                convexShapeList.Clear();
                convexShapeList.Add(Vector3.zero);
            
                convexShapeScreenPointList.Clear();

                int leftMostIndex = -1;
                float minX = float.MaxValue;
                for (int i = 0; i < pointsCount; i++)
                {
                    Vector3 screenPos = Camera.current.WorldToScreenPoint(pointsCloud[i]);

                    convexShapeScreenPointList.Add(screenPos);

                    if (screenPos.z < 0f) // Screen point is behind the camera
                        continue;

                    if (screenPos.x < minX)
                    {
                        leftMostIndex = i;
                        minX = screenPos.x;

                        convexShapeList[0] = pointsCloud[i];
                    }
                }

                if (leftMostIndex < 0)
                    return false;

                // Follow through the perimeter of the points cloud to get the convex shape
                Vector2 angleDirection = Vector2.right;
                Vector3 previousScreenPos = convexShapeScreenPointList[leftMostIndex];
                int previousPointIndex = leftMostIndex;
                int iterations = 0;
                int newPointIndex = -1;

                while (newPointIndex != leftMostIndex)
                {
                    float minAngle = float.MaxValue;
                    Vector3 newScreenPos = Vector3.zero;

                    for (int i = 0; i < pointsCount; i++)
                    {
                        if (i == previousPointIndex)
                            continue;

                        if (convexShapeScreenPointList[i].z < 0f) // Screen point is behind the camera
                            continue;

                        bool closeToPreviousPoint = ((Vector2)(convexShapeScreenPointList[i] - convexShapeScreenPointList[previousPointIndex])).magnitude < 0.1f;

                        if (closeToPreviousPoint)
                            continue;

                        float angle = Vector2.SignedAngle(angleDirection, convexShapeScreenPointList[i] - previousScreenPos);

                        if (Mathf.Abs(angle) > 179.5f) // To prevent some inaccuracies, especially with an orthographic view
                            continue;

                        if (angle < minAngle)
                        {
                            newPointIndex = i;
                            minAngle = angle;
                            newScreenPos = convexShapeScreenPointList[i];
                        }
                    }

                    if (newPointIndex < 0) // Something went wrong while trying to get the next point of the convex shape. Stop the while loop
                        return false;

                    bool closeToFirstPoint = ((Vector2)(convexShapeScreenPointList[newPointIndex] - convexShapeScreenPointList[leftMostIndex])).magnitude < 0.1f;
                    if (closeToFirstPoint) // To prevent some inaccuracies, especially with an orthographic view
                    {
                        newPointIndex = leftMostIndex;
                        newScreenPos = convexShapeScreenPointList[leftMostIndex];
                    }

                    iterations++;
                    if (iterations > maxConvexShapePoints) // Something went wrong while iterating through all the points. Stop the while loop
                        return false;

                    previousPointIndex = newPointIndex;
                    angleDirection = newScreenPos - previousScreenPos;
                    previousScreenPos = newScreenPos;

                    convexShapeList.Add(pointsCloud[newPointIndex]);
                }

                convexShapeArray = convexShapeList.ToArray();
            
                return true;
            }

            private static void DrawAnimatedCircle(Color color, Vector3 position, Vector3 normal, float radius, bool animated = true)
            {
                if (radius < 0f)
                    return;

                Color baseColor = Handles.color;

                if (animated)
                    radius *= timeWaveScaleMultiplier;

                Handles.color = color;
                Handles.DrawWireDisc(position, normal, radius);
                Handles.color = MultiplyColorAlpha(color, 0.05f);
                Handles.DrawSolidDisc(position, normal, radius);

                Handles.color = baseColor;
            }

            private static Vector3 MultiplyVector3(Vector3 v1, Vector3 v2)
            {
                return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
            }

            private static void TryDrawConvexShape(Vector3[] pointsCloud, Color color, bool getConvexShape)
            {
                if (getConvexShape)
                {
                    if (!GetConvexShape(pointsCloud))
                        return;
                }

                Color baseColor = Handles.color;
                
                Handles.color = color;
                Handles.DrawAAPolyLine(getConvexShape ? convexShapeArray : pointsCloud);

                Handles.color = MultiplyColorAlpha(color, convexShapeColorMultiplier);
                Handles.DrawAAConvexPolygon(getConvexShape ? convexShapeArray : pointsCloud);

                Handles.color = baseColor;
            }

            private static void GetSphereDisc(Vector3 position, float radius, out Vector3 discCenter, out Vector3 discDirection, out float discRadius)
            {
                discCenter = Vector3.zero;
                discDirection = Vector3.forward;
                discRadius = 0f;

                if (radius < Mathf.Epsilon)
                    return;

                if (!Camera.current)
                    return;

                if (Camera.current.orthographic)
                {
                    discCenter = position;
                    discDirection = Camera.current.transform.forward;
                    discRadius = radius;
                
                    return;
                }

                Vector3 camPos = Camera.current.transform.position;

                discDirection = position - camPos;
                Quaternion offsetRotation = Quaternion.LookRotation(discDirection, Camera.current.transform.up);

                Vector3 topPoint = position + offsetRotation * Vector3.up * radius;
                Vector3 bottomPoint = position - offsetRotation * Vector3.up * radius;

                Vector3 directionRight = Vector3.Cross(topPoint - bottomPoint, discDirection).normalized;

                Plane topPlane = new Plane(Vector3.Cross((topPoint - camPos).normalized, directionRight), topPoint);
                Vector3 topPlanePoint = topPlane.ClosestPointOnPlane(position);
                Vector3 topSurfacePoint = position + (topPlanePoint - position).normalized * radius;

                Plane bottomPlane = new Plane(Vector3.Cross((bottomPoint - camPos).normalized, directionRight), bottomPoint);
                Vector3 bottomPlanePoint = bottomPlane.ClosestPointOnPlane(position);
                Vector3 bottomSurfacePoint = position + (bottomPlanePoint - position).normalized * radius;

                discCenter = (topSurfacePoint + bottomSurfacePoint) * 0.5f;
                discRadius = (topSurfacePoint - bottomSurfacePoint).magnitude * 0.5f;
            }

            #endregion
        }
        
        #endif
    }
}