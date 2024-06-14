namespace snorri
{
    public abstract class Abilities : Ticker
    {
        protected Bag<Ability> abilities;
        protected override void Setup()
        {
            base.Setup();

            abilities = Node.GetBagOActors<Ability>();
        }
        protected override void Launch()
        {
            base.Launch();

            Choose();
        }

        // shortcuts:
        public Ability Current
        {
            get { return Node.Vars.Get<Ability>("ability_current", null); }
            set { 
                if (value == null)
                {
                    Node.Vars.Remove("ability_current");
                    Node.Vars.Remove("ability_current_name");
                } else
                {
                    Node.Vars.Set<Ability>("ability_current", value); 
                    Node.Vars.Set<string>("ability_current_name", value.Name); 
                }
                //LOG.Console("abilities change!"); 
                //Node.Vars.Log();
            }
        }

        // selection process:
        protected virtual void Choose()
        {
            Ability abilityCurrent = Node.Vars.Get<Ability>("ability_current", null);
            if (abilityCurrent == null)
            {
                Current = GetBestAbility();

                Current.Begin();

                // LOG.Console($"new ability chosen: {Current.Name}");

            } else
            {
                Ability abilityBest = GetBestAbility(abilityCurrent.Priority);
                if (abilityBest != abilityCurrent)
                {
                    abilityCurrent.End();

                    abilityBest.Begin();

                    // LOG.Console($"new ability chosen: {abilityBest.Name}");

                    Current = abilityBest;
                }
            }
        }

        // externals
        public void Clear()
        {
            if (Current != null)
                Current.End();

            Current = null;
        }

        // gets
        protected Ability GetBestAbility(int current_priority = -1)
        {
            Ability abilityBest = null;
            foreach (Ability ab in abilities)
            {
                if (ab.Priority > current_priority)
                {
                    if (ab.CanRun())
                    {
                        abilityBest = ab;
                        current_priority = ab.Priority;
                    }
                }
            }

            if (abilityBest == null)
                return Node.Vars.Get<Ability>("ability_current", GetBaseAbility());
                
            return abilityBest;
        }
        protected Ability GetBaseAbility()
        {
            foreach (Ability ab in abilities)
            {
                if (ab.Priority == 0)
                    return ab;
            }
            return null;
        }
    }
}