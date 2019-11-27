using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using Toys;

namespace ModelViewer
{
    static class PropertiesButtons
    {

        static HScale PutScale(string name, Fixed fixd, ref int offset)
        {
            Label lbl = new Label();
            lbl.Name = "lbl";
            lbl.Text = name;

            fixd.Put(lbl, 0, offset);
            lbl.Show();
            offset += 20;

            HScale scale = new HScale(0, 1, 0.1);
            scale.WidthRequest = 180;
            scale.Name = "scale";

            fixd.Put(scale, 0, offset);
            scale.Show();
            offset += 40;

            return scale;
        }

        static CheckButton PutCheckBox(string name, Fixed fixd, ref int offset)
        {
            /*
            Label lbl = new Label();
            lbl.Name = "lbl";
            lbl.Text = name;

            fixd.Put(lbl, 0, offset);
            lbl.Show();
            offset += 20;
            */
            CheckButton check = new CheckButton(name);
            check.WidthRequest = 180;
            check.Name = "check";

            fixd.Put(check, 0, offset);
            check.Show();
            offset += 40;

            return check;
        }


        static Entry PutInput(string name, Fixed fixd, ref int offset)
        {
            Label lbl = new Label();
            lbl.Name = "lbl";
            lbl.Text = name;

            fixd.Put(lbl, 0, offset);
            lbl.Show();
            offset += 20;

            Entry entry = new Entry();
            entry.WidthRequest = 180;
            entry.Name = "entry";
            fixd.Put(entry, 0, offset);
            entry.Show();
            offset += 40;

            return entry;
        }
    }
}
