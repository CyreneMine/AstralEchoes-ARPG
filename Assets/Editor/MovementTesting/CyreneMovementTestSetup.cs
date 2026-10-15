using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AstralEchoes.Testing.Editor
{
    public static class CyreneMovementTestSetup
    {
        private const string ControllerPath = "Assets/Resources/Animator/Cyrene_Blender4_2.controller";
        private const string ModelPath = "Assets/ArtRes/Character/Cyrene/Cyrene_Blender4.2.fbx";
        private const string Generated = "Assets/Resources/Animation/MovementTest";
        private const string Report = "Temp/AstralEchoesMovementSetup.result.json";

        [MenuItem("Tools/AstralEchoes/配置 WASD 移动测试")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("请先退出播放模式。");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/SampleScene.unity")
                throw new InvalidOperationException("请打开 SampleScene 后配置；工具不会切换或覆盖其他场景。");
            var animator = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Animator>(true))
                .Single(a => AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(a.gameObject)) == ModelPath);
            if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException("角色 Avatar 不是有效的 Humanoid，未修改 Animator。");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) throw new InvalidOperationException("找不到现有 Animator Controller。");
            var machine = controller.layers[0].stateMachine;
            var idle = machine.states.Select(s => s.state).Single(s => s.name == "KA_Combat_BareHands_Idle01");
            var moving = machine.states.Select(s => s.state).Single(s => s.motion is BlendTree);
            var tree = (BlendTree)moving.motion;
            if (!(idle.motion is AnimationClip idleClip) || !idleClip.isHumanMotion)
                throw new InvalidOperationException("现有 Idle 不是 Humanoid 动作。");

            EnsureFolder(Generated);
            var forward = CopyLoopingClip("Assets/Resources/Animation/Charecter/@KA_Run02.FBX", "MoveForward");
            const string source = "Assets/KAWAII_ANIMATIOMS_100/Assets/Animations/";
            var backward = CopyLoopingClip(source + "@KA_Run_Bwd.FBX", "MoveBackward");
            var left = CopyLoopingClip(source + "@KA_Run_Left.FBX", "MoveLeft");
            var right = CopyLoopingClip(source + "@KA_Run_Right.FBX", "MoveRight");
            Undo.RecordObject(tree, "配置四向移动混合树");
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "HSpeed";
            tree.blendParameterY = "VSpeed";
            tree.useAutomaticThresholds = false;
            tree.children = new[]
            {
                Child(idleClip, Vector2.zero), Child(forward, Vector2.up), Child(backward, Vector2.down),
                Child(left, Vector2.left), Child(right, Vector2.right)
            };
            foreach (string name in new[] { "speed", "HSpeed", "VSpeed" })
                if (!controller.parameters.Any(p => p.name == name)) controller.AddParameter(name, AnimatorControllerParameterType.Float);
            foreach (var t in idle.transitions.Where(t => t.destinationState == moving).ToArray()) idle.RemoveTransition(t);
            foreach (var t in moving.transitions.Where(t => t.destinationState == idle).ToArray()) moving.RemoveTransition(t);
            Transition(idle, moving, AnimatorConditionMode.Greater);
            Transition(moving, idle, AnimatorConditionMode.Less);
            machine.defaultState = idle;
            EditorUtility.SetDirty(tree);
            EditorUtility.SetDirty(controller);

            var character = animator.gameObject;
            Undo.RecordObject(animator, "绑定移动测试 Animator");
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var capsule = character.GetComponent<CharacterController>();
            if (capsule == null) capsule = Undo.AddComponent<CharacterController>(character);
            Undo.RecordObject(capsule, "配置测试角色碰撞");
            float headHeight = animator.GetBoneTransform(HumanBodyBones.Head).position.y - character.transform.position.y;
            float height = Mathf.Clamp(headHeight + 0.22f, 0.6f, 3f);
            capsule.height = height;
            capsule.radius = height * 0.15f;
            capsule.center = Vector3.up * height * 0.5f;
            capsule.skinWidth = Mathf.Max(0.01f, height * 0.01f);
            capsule.minMoveDistance = 0f;
            capsule.stepOffset = height * 0.15f;
            var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>()).First(c => c.CompareTag("MainCamera"));
            Undo.RecordObject(camera.transform, "配置移动测试跟随视角");
            camera.transform.position = character.transform.position + new Vector3(0f, height * 0.9f, -height * 2.5f);
            camera.transform.LookAt(character.transform.position + Vector3.up * height * 0.55f);
            var movement = character.GetComponent<CyreneMovementTest>();
            if (movement == null) movement = Undo.AddComponent<CyreneMovementTest>(character);
            var serialized = new SerializedObject(movement);
            serialized.FindProperty("animator").objectReferenceValue = animator;
            serialized.FindProperty("movementCamera").objectReferenceValue = camera;
            serialized.FindProperty("inputActions").objectReferenceValue = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            serialized.ApplyModifiedProperties();

            if (!scene.GetRootGameObjects().Any(g => g.name == "MovementTestGround"))
            {
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                Undo.RegisterCreatedObjectUndo(ground, "创建移动测试地面");
                ground.name = "MovementTestGround";
                ground.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, character.transform.position.z);
                ground.transform.localScale = Vector3.one * 4f;
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor", new Color(0.25f, 0.29f, 0.32f));
                const string groundPath = Generated + "/TestGround.mat";
                var existing = AssetDatabase.LoadAssetAtPath<Material>(groundPath);
                if (existing == null) { AssetDatabase.CreateAsset(material, groundPath); existing = material; }
                else UnityEngine.Object.DestroyImmediate(material);
                ground.GetComponent<Renderer>().sharedMaterial = existing;
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("场景保存失败。");
            File.WriteAllText(Report, JsonUtility.ToJson(new SetupReport { configured = true, avatarValid = true, scene = scene.path, height = height }, true));
            Selection.activeGameObject = character;
            Debug.Log("[AstralEchoes] WASD 移动测试配置完成。");
        }

        private static ChildMotion Child(Motion clip, Vector2 point) => new ChildMotion { motion = clip, position = point, timeScale = 1f };

        private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.12f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(mode, 0.1f, "speed");
        }

        private static AnimationClip CopyLoopingClip(string path, string name)
        {
            var source = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(c => !c.name.StartsWith("__preview__"));
            if (!source.isHumanMotion) throw new InvalidOperationException(path + " 不是 Humanoid 动作。");
            string output = Generated + "/" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(output);
            if (clip == null)
            {
                clip = UnityEngine.Object.Instantiate(source);
                clip.name = name;
                AssetDatabase.CreateAsset(clip, output);
            }
            // 保留源动作的根运动提取设置，不能把有位移的跑步轨迹烘焙进身体姿态。
            var settings = AnimationUtility.GetAnimationClipSettings(source);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void EnsureFolder(string path)
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        [Serializable] private class SetupReport { public bool configured; public bool avatarValid; public string scene; public float height; public string error; }

    }
}
