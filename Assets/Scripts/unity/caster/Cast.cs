namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;

    public class Cast : Element, IMap
    {
        public Map Vars {get; set;}

        public Bag<Map> Results { 
            get {
                return Vars.Get<Bag<Map>>("results", new Bag<Map>());
            }
        }
        public int Hits {
            get {
                return Vars.Get<int>("results_count", 0);
            }
        }
        public Vec GetNormal(int i)
        {
            if (i >= Hits)
                return Vec.zero;

            Map m = Results[i];
            return m.Get<Vec>("hit_normal", Vec.zero);
        }
        public Map GetClosest(Vec position)
        {
            float distanceMin = 10000f;
            Map hitClosest = null;
            foreach (Map hit in Results)
            {
                float distance = hit.Get<float>("hit_distance");
                if (distance < distanceMin)
                {
                    distanceMin = distance;
                    hitClosest = hit;
                }   
            }
            return hitClosest;
        }
        public Vec Position {
            get { 
                Vec position = null;
                float distance = 1000f;
                foreach (Map result in Results)
                {
                    float distanceTemp = result.Get<float>("hit_distance", 1000f);
                    if (distanceTemp < distance)
                    {
                        distance = distanceTemp;
                        position = result.Get<Vec>("hit_position", Vec.zero);
                    }
                }

                return position;
            }
        }
        public float Distance {
            get { 
                float distance = 1000f;
                foreach (Map result in Results)
                {
                    float distanceTemp = result.Get<float>("hit_distance", 1000f);
                    if (distanceTemp < distance)
                    {
                        distance = distanceTemp;
                    }
                }

                return distance;
            }
        }
        
        public Cast(Map args)
        {
            Vars = new Map();
            Vars.Set<Map>("args", args);

            Vec origin = new Vec(args.Get<Bag<float>>("origin", new Bag<float>(0f,0f,0f)));
            Vec end = new Vec(args.Get<Bag<float>>("end", new Bag<float>(0f,0f,0f)));
            Vec rotation = new Vec(args.Get<Bag<float>>("rotation", new Bag<float>(0f,0f,0f)));
            Vec extents = new Vec(args.Get<Bag<float>>("extents", new Bag<float>(0f,0f,0f)));

            float distance = args.Get<float>("distance", 100f);

            Vec direction = new Vec(args.Get<Bag<float>>("direction", new Bag<float>(0f,0f,1f)));

            Bag<string> maskNames = args.Get<Bag<string>>("layer_mask", new Bag<string>());

            string type = args.Get<string>("type", "ray");

            float radius = args.Get<float>("radius", 1f);

            //LOG.Console($"new cast has type: {type}, origin: {origin.vec3}");

            LayerMask mask = LayerMask.GetMask(maskNames.All());

            RaycastHit[] hits = null;
            switch (type)
            {
                case "sphere":
                    hits = Physics.SphereCastAll(origin.vec3, radius, direction.vec3, distance, mask);
                    break;
                case "capsule":
                    hits = Physics.CapsuleCastAll(origin.vec3, end.vec3, radius, direction.vec3, distance, mask);
                    break;
                case "box":
                    hits = Physics.BoxCastAll(origin.vec3, extents.vec3, direction.vec3, rotation.quat, distance, mask);
                    break;
                case "ray":
                    hits = Physics.RaycastAll(origin.vec3, direction.vec3, distance, mask);
                    break;
            }

            // LOG.Console($"new cast, origin of {origin.vec3}, direction of {direction.vec3}, with results: {hits.Length}");

            Bag<Map> bagOfResults = new Bag<Map>();
            foreach (RaycastHit hit in hits)
            {   
                NodeEntity e = hit.collider.gameObject.GetComponent<NodeEntity>();
                Map result = new Map();

                if (e != null)
                {
                    Node n = e.Node;
                    result.Set<Node>("hit", n);
                }

                //if (Vector3.Distance(hit.point, Vector3.zero) < 0.05f)
                //{
                //    LOG.Console("cast has distance: " + hit.point);
                //}
                
                result.Set<Vec>("hit_position", new Vec(hit.point));
                result.Set<Collider>("hit_collider", hit.collider);
                result.Set<float>("hit_distance", hit.distance);
                result.Set<Vec>("hit_normal", new Vec(hit.normal));

                bagOfResults.Append(result);
            }

            Vars.Set<Bag<Map>>("results", bagOfResults);
            Vars.Set<int>("results_count", bagOfResults.Length);
        }
    }
    
}