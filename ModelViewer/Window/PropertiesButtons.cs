using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using Toys;
using OpenTK.Mathematics;
using System.Reflection;

namespace ModelViewer
{
    static class PropertiesButtons
    {        
        public static void DrawField(Component component, FieldInfo field, Fixed fixd, ref int offset)
        {
            if (field.FieldType == typeof(int))
                PutSingleInput(component, field, InputPurpose.Digits, fixd, ref offset);
            else if (field.FieldType == typeof(float))
                PutSingleInput(component, field, InputPurpose.Number, fixd, ref offset);
            else if (field.FieldType == typeof(string))
                PutSingleInput(component, field, InputPurpose.FreeForm, fixd, ref offset);
            
            //if (field.FieldType == typeof(Vector4))


        }


        public static void DrawField(Component component, PropertyInfo field, Fixed fixd, ref int offset)
        {
            if (field.PropertyType == typeof(int))
                PutSingleInput(component, field, InputPurpose.Digits, fixd, ref offset);
            else if (field.PropertyType == typeof(float))
                PutSingleInput(component, field, InputPurpose.Number, fixd, ref offset);
            else if (field.PropertyType == typeof(string))
                PutSingleInput(component, field, InputPurpose.FreeForm, fixd, ref offset);
            else if (field.PropertyType == typeof(Vector2))
                PutVectorInput(component, field, fixd, ref offset);
            else if (field.PropertyType == typeof(Vector3))
                PutVectorInput(component, field, fixd, ref offset);
            else if (field.PropertyType == typeof(Vector4))
                PutVectorInput(component, field, fixd, ref offset);
            else if (field.PropertyType == typeof(IVector4))
                PutVectorInput(component, field, fixd, ref offset);

        }


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
            CheckButton check = new CheckButton(name);
            check.WidthRequest = 180;
            check.Name = "check";

            fixd.Put(check, 0, offset);
            check.Show();
            offset += 40;

            return check;
        }


        static Entry PutSingleInput(Component component, FieldInfo field, InputPurpose  fillter, Fixed fixd, ref int offset)
        {
            //Label
            Label lbl = new Label();
            lbl.Name = "lbl";
            lbl.Text = field.Name;

            fixd.Put(lbl, 0, offset);
            lbl.Show();
            offset += 20;

            //Field
            Entry entry = new Entry();
            entry.WidthRequest = 180;
            entry.Name = "entry";
            fixd.Put(entry, 0, offset);
            entry.Show();
            entry.InputPurpose = fillter;
            offset += 40;
            entry.Text = field.GetValue(component).ToString();
            
            entry.Changed += (s, e) => {
                if (field.FieldType == typeof(int))
                {
                    int value = 0;
                    int.TryParse(entry.Text, out value);
                    field.SetValue(component, value);
                }
                else if (field.FieldType == typeof(float))
                {
                    float value = 0;
                    float.TryParse(entry.Text, out value);
                    field.SetValue(component, value);
                }
                else if (field.FieldType == typeof(string))
                    field.SetValue(component, entry.Text);
            };
            
            return entry;
        }

        static Entry PutSingleInput(Component component, PropertyInfo field, InputPurpose fillter, Fixed fixd, ref int offset)
        {
            //Label
            Label lbl = new Label();
            lbl.Name = "lbl";
            lbl.Text = field.Name;

            fixd.Put(lbl, 0, offset);
            lbl.Show();
            offset += 20;

            //Field
            Entry entry = new Entry();
            entry.WidthRequest = 180;
            entry.Name = "entry";
            fixd.Put(entry, 0, offset);
            entry.Show();
            entry.InputPurpose = fillter;
            offset += 40;
            entry.Text = field.GetValue(component).ToString();

            entry.Changed += (s, e) => {
                if (field.PropertyType == typeof(int))
                {
                    int value = 0;
                    int.TryParse(entry.Text, out value);
                    field.SetValue(component, value);
                }
                else if (field.PropertyType == typeof(float))
                {
                    float value = 0;
                    float.TryParse(entry.Text, out value);
                    field.SetValue(component, value);
                }
                else if (field.PropertyType == typeof(string))
                    field.SetValue(component, entry.Text);
            };

            return entry;
        }


        static void PutVectorInput(Component component, PropertyInfo property, Fixed fixd, ref int offset)
        {
            Label lbl = new Label();
            lbl.Name = "lbl";
            lbl.Text = property.Name;

            string[] namings = { "x", "y", "z", "w" };

            fixd.Put(lbl, 0, offset);
            lbl.Show();
            offset += 20;
            
            int count = 0;
            InputPurpose purpose = InputPurpose.Number;
            if (property.PropertyType == typeof(Vector2))
                count = 2;  
            else if (property.PropertyType == typeof(Vector3))
                count = 3;
            else if (property.PropertyType == typeof(Vector4))
                count = 4;



            int offsetX = 0;
            for (int i = 0; i < count; i++)
            {
                Label lbl1 = new Label();
                lbl1.Name = "lbl";
                lbl1.Text = namings[i];
                fixd.Put(lbl1, offsetX, offset);
                lbl1.Show();
                offsetX += 15;

                Entry entry = new Entry();
                entry.WidthChars = 4;
                entry.Name = "entry";
                entry.InputPurpose = purpose;

                int n = i;
                if (property.PropertyType == typeof(Vector2))
                { 
                    Vector2 val = (Vector2)property.GetValue(component);
                    entry.Text = val[i].ToString();
                    if (property.SetMethod != null)
                    {
                        entry.Changed += (s, e) =>
                        {
                            float value = 0;
                            float.TryParse(entry.Text, out value);
                            val[n] = value;
                            property.SetValue(component, val);
                        };
                    }
                }
                else if (property.PropertyType == typeof(Vector3))
                {
                    Vector3 val = (Vector3)property.GetValue(component);
                    entry.Text = val[i].ToString();
                    if (property.SetMethod != null)
                    {
                        entry.Changed += (s, e) =>
                        {
                            float value = 0;
                            float.TryParse(entry.Text, out value);
                            val[n] = value;
                            property.SetValue(component, val);
                        };
                    }
                }
                else if (property.PropertyType == typeof(Vector4))
                {
                    Vector4 val = (Vector4)property.GetValue(component);
                    entry.Text = val[i].ToString();
                    if (property.SetMethod != null)
                    {
                        entry.Changed += (s, e) =>
                            {
                              float value = 0;
                              float.TryParse(entry.Text, out value);
                              val[n] = value;
                              property.SetValue(component, val);
                            };
                    }
                }
                fixd.Put(entry, offsetX, offset);
                entry.Show();
                offsetX += 45;
            }
            offset += 40;
        }

        static void SetVectorValue(Component component, int pos, PropertyInfo property, Entry entry, InputPurpose purpose, ref float val)
        {
            entry.Text = val.ToString();

            entry.Changed += (s, e) =>
            {
                if (purpose == InputPurpose.Number)
                {
                    float value = 0;
                    float.TryParse(entry.Text, out value);
                    property.SetValue(component, value);
                }
                else
                {
                    int value = 0;
                    int.TryParse(entry.Text, out value);
                    property.SetValue(component, value);
                }
            };
        }
    }
}
