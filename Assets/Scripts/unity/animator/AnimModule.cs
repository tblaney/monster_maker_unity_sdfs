using UnityEngine;

namespace snorri
{
    public class AnimModule : Module
    {
        Animator animator;
        AnimState state;

        Bag<string> layers;
        public Bag<string> Layers
        {
            get {
                return layers;
            }
        }

        public bool IsMatchingTarget
        {
            get { return animator.isMatchingTarget; }
        }
        public bool Root
        {
            get { return animator.applyRootMotion; }
            set { animator.applyRootMotion = value; }
        }

        void LateUpdate()
        {
            StateUpdate();
        }
        void OnAnimatorMove()
        {
            MoveUpdate();
        }

        protected override void AddClasses()
        {
            base.AddClasses();

            animator = ComponentCheck<Animator>();
        }
        protected override void Setup()
        {
            base.Setup();

            state = new AnimState(this, Vars.Get<Map>("state", new Map()));
            layers = Vars.Get<Bag<string>>("layers", new Bag<string>("base"));
        }


        void StateUpdate()
        {
            state.Update();
        }
        void MoveUpdate()
        {

        }

        public void SetAnim(string stateName, float transitionDuration = 0.2f)
        {
            Map animationMap = state.GetAnimationMap(stateName);

            if (animationMap == null)
                return;

            for (int i = 0; i < layers.Length; i++)
            {
                animator.CrossFadeInFixedTime(state.Format(animationMap.Get<string>("name", ""), layers[i]), transitionDuration, i);
            }
        }

        public bool IsTransition(int index = 0)
        {
            return animator.IsInTransition(index);
        }
        public void MatchTarget(Vector3 matchingPosition, Quaternion matchingRotation, AvatarTarget avatarTarget, MatchTargetWeightMask weightMask, float startTime, float targetTime)
        {
            animator.MatchTarget(matchingPosition, matchingRotation, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), startTime, targetTime);
        }
        public void SetBool(string Name, bool value)
        {
            animator.SetBool(Name, value);
        }
        public void SetFloat(string Name, float value, float dampTime, float deltaTime)
        {
            animator.SetFloat(Name, value, dampTime, deltaTime);
        }
        public void SetFloat(int id, float value, float dampTime, float deltaTime)
        {
            animator.SetFloat(id, value, dampTime, deltaTime);
        }
        public void SetFloat(string Name, float value)
        {
            animator.SetFloat(Name, value);
        }
        public void SetInt(string Name, int value)
        {
            animator.SetInteger(Name, value);
        }
        public float GetFloat(string Name)
        {
            return animator.GetFloat(Name);
        }
        AnimatorStateInfo GetStateInfo(int index = 0)
        {
            return animator.GetCurrentAnimatorStateInfo(index);
        }
        public Bag<int> GetCurrentHashes()
        {
            Bag<int> hashes = new Bag<int>();
            for (int i = 0; i < layers.Length; i++)
            {
                hashes.Append(GetStateInfo(i).nameHash);
            }
            return hashes;
        }
        AnimatorClipInfo[] GetClipInfo(int index = 0)
        {
            return animator.GetCurrentAnimatorClipInfo(index);
        }
        public Bag<string> GetClips()
        {
            AnimatorClipInfo[] infos = GetClipInfo();
            Bag<string> currentAnimationsByLayer = new Bag<string>();
            foreach (AnimatorClipInfo info in infos)
            {
                currentAnimationsByLayer.Append(info.clip.name);
            }
            return currentAnimationsByLayer;
        }
        public bool GetBool(string Name)
        {
            return animator.GetBool(Name);
        }
        public float GetNormalizedTime()
        {
            AnimatorStateInfo info = GetStateInfo();
            float time = info.normalizedTime;
            if (info.normalizedTime > 1f)
            {
                time = time - (float)(Mathf.FloorToInt(info.normalizedTime));
            }
            return time;
        }
        public AnimState GetState()
        {
            return state;
        }
        public bool IsAnimation(string anim)
        {
            return state.IsAnimation(anim);
        }
        public int Hash(string text)
        {
            return Animator.StringToHash(text);
        }
    }

    public class AnimState
    {
        [Header("define animations here:")]
        Bag<Map> animations;

        public Map GetAnimationMap(string name)
        {
            foreach (Map m in animations)
            {
                if (name == m.Get<string>("name", ""))
                    return m;
            }
            return null;
        }

        string[] currentAnimationsByLayer;

        AnimModule module;
        Map vars;

        public AnimState(AnimModule module, Map args)
        {
            this.module = module;
            vars = args;

            Init();
        }
        public void Init()
        {
            animations = vars.Get<Bag<Map>>("animations", new Bag<Map>());
            foreach (Map anim in animations)
            {
                Bag<int> hashes = new Bag<int>();
                foreach (string layer in anim.Get<Bag<string>>("layers", new Bag<string>("base")))
                {
                    hashes.Append(module.Hash(System.String.Format("{0}.{1}", layer, anim.Get<string>("name", ""))));
                }
                anim.Set<Bag<int>>("hashes", hashes);
            }
        }
        public void Update()
        {
            AnimationUpdate();
        }
        void AnimationUpdate()
        {
            Bag<int> hashesCurrent = module.GetCurrentHashes();
            //hashes current has length reflective of active layers

            currentAnimationsByLayer = new string[module.Layers.Length];

            void InsertCurrent(string animIn, int index)
            {
                if (index >= currentAnimationsByLayer.Length)
                {
                    return;
                } else
                {
                    currentAnimationsByLayer[index] = animIn;
                }
            }

            int i = 0;
            foreach (int hash in hashesCurrent)
            {
                foreach (Map anim in animations)
                {
                    bool foundHash = false;
                    foreach (int animHash in anim.Get<Bag<int>>("hashes", new Bag<int>()))
                    {
                        if (hash == animHash)
                        {
                            //_animation = anim.Name;
                            InsertCurrent(anim.Get<string>("name"), i);
                            foundHash = true;
                            break;
                        }
                    }
                    if (foundHash)
                        break;
                }
                i++;
            }

            if (currentAnimationsByLayer.Length > 0)
                LOG.Console($"animation update has current animation: {currentAnimationsByLayer[0]}");
        }
        public string Format(string anim, string layer = "base")
        {
            return System.String.Format("{0}.{1}", layer, anim);
        }
        public bool IsAnimation(string anim)
        {
            foreach (string currentAnim in currentAnimationsByLayer)
            {
                if (currentAnim == anim)
                    return true;
            }
            return false;
        }
    }
}
