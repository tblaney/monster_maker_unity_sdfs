namespace snorri
{
    public class CastTicker : Ticker
    {
        bool isAwarenessCast = false;

        public Cast this[string name]
        {
            get 
            {
                if (name == "awareness")
                {
                    return GetAwarenessCast();
                }
                bool isUpdate = Vars.Get<bool>(name + ":is_update", false);
                if (!isUpdate)
                {
                    ProcessRequest(Vars.Get<Map>(name, new Map()));
                }
                return Vars.Get<Cast>(name + ":cast", null);
            }
        }
        public Cast GetCast(string name, Map argsIn = null, bool isForceUpdate = false)
        {
            if (name == "awareness")
            {
                return GetAwarenessCast();
            }
            bool isUpdate = Vars.Get<bool>(name + ":is_update", false);
            if ((!isUpdate && isForceUpdate) || argsIn != null)
            {
                Map requestMap = Vars.Get<Map>(name, new Map());
                if (argsIn != null)
                {
                    requestMap.Sync(argsIn);
                }
                ProcessRequest(requestMap);
            }
            return Vars.Get<Cast>(name + ":cast", null);
        }
        protected override void Setup()
        {
            base.Setup();
        }
        public override void Tick()
        {
            base.Tick();

            int i = 0;
            foreach (string requestName in Vars.Elements.Keys)
            {
                Map request = Vars.Get<Map>(requestName, new Map());

                bool isUpdate = request.Get<bool>("is_update", false);
                if (!isUpdate)
                    continue;

                ProcessRequest(Vars.Get<Map>(requestName, new Map()));

                LOG.Console($"cast ticker has cast: {requestName}");

                i++;
            }    

            //LOG.Console($"cast ticker has {i} casts!");
            //Vars.Log();
        }

        void ProcessRequest(Map request)
        {
            Map argsBase = request.Get<Map>("args", new Map());

            Map args = new Map();
            args.Sync(argsBase);

            if (args.Get<bool>("is_origin_local", false))
            {
                Bag<float> originCurrent = argsBase.Get<Bag<float>>("origin", new Bag<float>(0,0,0));
                Bag<float> originNew = new Bag<float>(originCurrent.All());
                originNew[0] = Node.Point.East.x*originNew[0] + Node.Point.Position.x;
                originNew[1] = originNew[1] + Node.Point.Position.y;
                originNew[2] = Node.Point.North.z*originNew[2] + Node.Point.Position.z;
                args.Set<Bag<float>>("origin", originNew);
            }

            string directionName = args.Get<string>("direction_local", "");
            if (directionName != "")
            {
                switch (directionName)
                {
                    case "forward":
                        Vec north = Node.Point.North;
                        north.y = 0f;
                        args.Set<Bag<float>>("direction", north.AsBag());
                        break;
                    case "south":
                        args.Set<Bag<float>>("direction", Node.Point.South.AsBag());
                        break;
                    case "east":
                        args.Set<Bag<float>>("direction", Node.Point.East.AsBag());
                        break;
                    case "west":
                        args.Set<Bag<float>>("direction", Node.Point.West.AsBag());
                        break;
                }
            }

            request.Set<Cast>("cast", new Cast(args));
        }

        Cast GetAwarenessCast()
        {
            Bag<Map> requests = new Bag<Map>();
            for (int i = 0; i < 6; i++)
            {
                Map args = new Map();
                Vec origin = Node.Point.Position;
                args.Set<Bag<float>>("args:origin", new Bag<float>(origin.x,origin.y,origin.z));
                
                switch (i)
                {
                    case 0:
                        args.Set<Bag<float>>("args:direction", new Bag<float>(0f,1f,0f));
                        args.Set<string>("args:name", "up");
                        break;
                    case 1:
                        args.Set<Bag<float>>("args:direction", new Bag<float>(0f,-1f,0f));
                        args.Set<string>("args:name", "down");
                        break;
                    case 2:
                        args.Set<Bag<float>>("args:direction", new Bag<float>(1f,0f,0f));
                        args.Set<string>("args:name", "right");
                        break;
                    case 3:
                        args.Set<Bag<float>>("args:direction", new Bag<float>(-1f,0f,0f));
                        args.Set<string>("args:name", "left");
                        break;
                    case 4:
                        args.Set<Bag<float>>("args:direction", new Bag<float>(0f,0f,1f));
                        args.Set<string>("args:name", "forward");
                        break;
                    case 5:
                        args.Set<Bag<float>>("args:direction", new Bag<float>(0f,0f,-1f));
                        args.Set<string>("args:name", "back");
                        break;
                }

                args.Set<float>("args:distance", 20f);
                args.Set<Bag<string>>("args:layer_mask", new Bag<string>("Terrain"));
                args.Set<string>("args:type", "ray");

                ProcessRequest(args);

                Cast cast = args.Get<Cast>("cast");
                if (cast.Hits > 0)
                {
                    LOG.Console($"caster awareness with name {args.Get<string>("args:name")}, has cast hit: {cast.Position.vec3}");
                }

                requests.Append(args, false);
            }

            Map requestOut = new Map();
            float distanceMin = 10000f;
            Cast c = null;

            foreach (Map m in requests)
            {
                if (!m.Has("cast"))
                    continue;

                Cast cast = m.Get<Cast>("cast");

                if (cast.Hits == 0)
                    continue;

                float distance = m.Get<Cast>("cast", null).Distance;
                LOG.Console("cast ticker awareness, distance: " + distance);
                if (distance < distanceMin)
                {
                    distanceMin = distance;
                    c = m.Get<Cast>("cast");
                }
            }

            return c;
        }
    }
}