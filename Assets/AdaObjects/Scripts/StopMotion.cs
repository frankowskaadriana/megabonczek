using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace GK
{
    public class StopMotion : MonoBehaviour
    {
        public List<Transform> RootBones = new List<Transform>();
        public int StoppedFrameCount = 5;
        public bool AutoFindBones = true;

        int recordedFrame = -1;
        List<Transform> transforms = null;
        List<STransform> actualPositions = null;
        List<STransform> renderedPositions = null;
        IEnumerator endOfFrameCoroutine;

        void OnEnable()
        {
            transforms = null;
            actualPositions = null;
            renderedPositions = null;
            endOfFrameCoroutine = EndOfFrameCoroutine();
            StartCoroutine(endOfFrameCoroutine);
        }

        void OnDisable()
        {
            StopCoroutine(endOfFrameCoroutine);
        }

        void LateUpdate()
        {
            if (transforms == null)
            {
                GatherAllBones();
            }

            RecordTransform(ref actualPositions);

            if (renderedPositions == null || Time.frameCount - recordedFrame >= StoppedFrameCount)
            {
                recordedFrame = Time.frameCount;
                RecordTransform(ref renderedPositions);
            }
            else
            {
                RestoreRecord(renderedPositions);
            }
        }

        void GatherAllBones()
        {
            transforms = new List<Transform>();

            if (RootBones.Count > 0)
            {
                // Use manually specified root bones
                foreach (var root in RootBones)
                {
                    if (root != null)
                    {
                        transforms.AddRange(root.GetComponentsInChildren<Transform>());
                    }
                }
            }
            else if (AutoFindBones)
            {
                // Auto-detect
                var smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smrs.Length > 0)
                {
                    // Use skinned mesh bones
                    foreach (var smr in smrs)
                    {
                        transforms.AddRange(smr.bones);
                        if (smr.rootBone != null && !transforms.Contains(smr.rootBone))
                        {
                            transforms.Add(smr.rootBone);
                        }
                    }
                }
                else
                {
                    // Find all transforms with bone-like names or children
                    var allTransforms = GetComponentsInChildren<Transform>();
                    foreach (var t in allTransforms)
                    {
                        if (t.parent != null &&
                            (t.name.ToLower().Contains("bone") ||
                             t.parent.name.ToLower().Contains("armature") ||
                             t.parent.name.ToLower().Contains("root")))
                        {
                            transforms.Add(t);
                        }
                    }
                }
            }

            // Remove duplicates while preserving order
            var uniqueTransforms = new HashSet<Transform>();
            transforms = transforms.Where(t => t != null && uniqueTransforms.Add(t)).ToList();
        }

        IEnumerator EndOfFrameCoroutine()
        {
            var endOfFrame = new WaitForEndOfFrame();

            while (true)
            {
                yield return endOfFrame;
                RestoreRecord(actualPositions);
            }
        }

        void RecordTransform(ref List<STransform> record)
        {
            if (record == null)
            {
                record = new List<STransform>(transforms.Count);

                foreach (var t in transforms)
                {
                    record.Add(STransform.FromTransform(t));
                }
            }
            else
            {
                for (int i = 0; i < transforms.Count; i++)
                {
                    record[i] = STransform.FromTransform(transforms[i]);
                }
            }

            Debug.Assert(transforms.Count == record.Count);
        }

        void RestoreRecord(List<STransform> record)
        {
            Debug.Assert(record != null);
            Debug.Assert(record.Count == transforms.Count);

            for (int i = 0; i < transforms.Count; i++)
            {
                record[i].WriteTo(transforms[i]);
            }
        }

        void Reset()
        {
            var smr = GetComponentInChildren<SkinnedMeshRenderer>();

            if (smr != null)
            {
                // Try to get all root bones
                var allBones = smr.bones;
                if (allBones.Length > 0)
                {
                    RootBones.Clear();
                    // Add unique root bones
                    foreach (var bone in allBones)
                    {
                        if (bone.parent == null || !allBones.Contains(bone.parent))
                        {
                            RootBones.Add(bone);
                        }
                    }
                }
            }

            StoppedFrameCount = 5;
        }

        struct STransform
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
            public Vector3 LocalScale;

            public static STransform FromTransform(Transform t)
            {
                return new STransform
                {
                    LocalPosition = t.localPosition,
                    LocalRotation = t.localRotation,
                    LocalScale = t.localScale
                };
            }

            public void WriteTo(Transform t)
            {
                t.localPosition = LocalPosition;
                t.localRotation = LocalRotation;
                t.localScale = LocalScale;
            }
        }
    }
}