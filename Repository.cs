using System;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

public class Repository
{
    private readonly string path;
    public Repository(string path) => this.path = path;
    public void Add(string title, string desc, int hora)
    {
        XDocument _xDocument = XDocument.Load(path);
        int maxId;
        
        try
        {
           maxId = _xDocument.Root.Elements().Max(x => (int)x.Element("Id")) + 1;
        }
        catch
        {
            maxId = 1;
        }

        title = !string.IsNullOrEmpty(title) ? title : "Sin establecer";
        desc = !string.IsNullOrEmpty(desc) ? desc : "Sin establecer";

        XElement newtarea = new XElement("Tarea",
        new XElement("Id", maxId),
        new XElement("Titulo", title),
        new XElement("Hora", hora),
        new XElement("Descripcion", desc));

        var element = _xDocument.Descendants("Tarea");
        if(element.Any())
        {
            element.Last().AddAfterSelf(newtarea);
        }
        else
        {
            _xDocument.Root.Add(newtarea);
        }
        
        _xDocument.Save(path);
         SucessMessge("Tarea agregada con exito");
    }

    public void DeleteEntry(int id)
    {
        XDocument _xDocument = XDocument.Load(path);
        var tarea = _xDocument.Root.Elements()
                             .Where(x => int.Parse(x.Element("Id").Value) == id);
        if (!tarea.Any())
        {
            FailReset(id);
        }
        else
        {
            tarea.Remove();
            _xDocument.Save(path);
            SucessMessge("Tarea eliminada con exito");
        }
    }


    public bool ElementIsValid(int id)
    {
        return XDocument.Load(path).Root.Elements().Where(x =>
                               int.Parse(x.Element("Id").Value) == id).Any();
    }

    public void EditEntry(int id, string titulo, string descripcion, int hora)
    {
        XDocument _xDocument = XDocument.Load(path);
        var tarea = _xDocument.Root.Elements().SingleOrDefault(x =>
                              int.Parse(x.Element("Id").Value) == id);
        if (tarea == null)
        {
            FailReset(id);
        }
        else
        {
            tarea.SetElementValue("Hora", hora);
            tarea.SetElementValue("Titulo", titulo);
            tarea.SetElementValue("Descripcion", descripcion);
            _xDocument.Save(path);              
            SucessMessge("Tarea editada con exito");
        }
    }

    private void FailReset(int id)
    {
        ErrorMessge("Tarea de Id " + id + " no valida");
        System.Threading.Thread.Sleep(1000);
    }

 
    public void DisplayList()
    {
        var tareas = GetTareas();
        Console.WriteLine("-----------------------------------------------------------------------------------------------------");
        Console.WriteLine("Id     |  Título                             | Hora     | Descripción");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------");
        foreach (DataRowView dr in tareas)
        {
            string horaformat= TimeFormat(dr["Hora"].ToString());

            Console.WriteLine($"{dr["Id"].ToString()}        {dr["Titulo"].ToString().PadRight(38, ' ')}{horaformat.PadRight(10,' ')}{dr["Descripcion"].ToString()}");
        }
    }

    public DataView GetTareas()
    {
        DataSet ds = new DataSet();
        ds.ReadXml(path);
        DataView tareas;
        tareas = ds.Tables[0].DefaultView;
        tareas.Sort = "Id";
        return tareas;
    }

    public int TareasCantidad
    {
        get
        {
            try
            {
                var taras = GetTareas();
                return taras.Count;
            }
            catch
            {
                return 0;
            }
        }
    }
    public void NormalConsoleText(string txt) => Console.Write(txt);
    public void ConsoleText(string txt) => Console.WriteLine(txt);
    public void ErrorMessge(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        ConsoleText(msg);
        Console.ResetColor();
    }

    public void SucessMessge(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        ConsoleText(msg);
        Console.ResetColor();
    }

    public void WelComsoleVelozTODO() => Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Welcome Veloz TODO"));
    public string TimeFormat(string hora) 
    {
        int time = int.Parse(hora);
        string formataddcero = time < 10 ? "0" : "";
        
        return  time <= 11 ? $"{formataddcero}{hora} a.m." : $"{formataddcero}{hora} p.m.";
    }
}