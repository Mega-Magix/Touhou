using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Text;
using System.Threading.Tasks;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace IronPython
{
    class IronPython
    {
        public void RunFile(String filename)
        {
            var ipy = Python.CreateRuntime();
            dynamic test = ipy.UseFile("../../../PythonScripts/" + filename);
            test.main();
        }
    }
}
