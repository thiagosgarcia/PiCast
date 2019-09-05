using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiCast.Model
{
    public class Entity
    {
        public int Id { get; set; }
    }

    public class Configuration : Entity
    {
        public string Name { get; set; }
        public string Context { get; set; }
        public string Value { get; set; }
    }
}
