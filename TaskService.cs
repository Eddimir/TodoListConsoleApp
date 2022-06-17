using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
public class TaskService 
{
    private readonly string EmailHost = "", PasswordHost = "";
    public enum Accion {Nuevo, Editar, Eliminar}

    private Repository _repository;
    private string correoelectronico;
    public TaskService(string path)
    {
        _repository = new Repository(path);
    }
    public void ManagerTaskItem()
    {
        _repository.WelComsoleVelozTODO();
        while (true)
        {            
            if (_repository.TareasCantidad > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                _repository.ConsoleText("\n---------------------------------" + DateTime.Now.ToLongDateString() + "-------------------------------");
                string desc = _repository.TareasCantidad > 1 ? " tareas" : " tarea";
                _repository.ConsoleText("---------------------------------------Cuentas con " + _repository.TareasCantidad + desc +"---------------------------------------");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                _repository.ConsoleText("\tNota: Las horas son representados en formato de 24, las 20 representa las 8 p.m. etc...");
                Console.ForegroundColor = ConsoleColor.Green;
                _repository.ConsoleText("\t¿Que deseas hacer? Elige una de las siguientes opciones encerradas en parentesis.");
                Console.WriteLine();
                _repository.ConsoleText("(1) para agregar, (2) para editar, (3) para eliminar, (4) para mostrar tareas y (5) para finalizar");            
                string valor = Console.ReadLine();
                
                Console.ResetColor();
                Console.Clear();

                switch (valor)
                {
                    case "1": TaskManagerByAccion(Accion.Nuevo);
                        break;
                    case "2": TaskManagerByAccion(Accion.Editar);
                        break;
                    case "3": TaskManagerByAccion(Accion.Eliminar);
                        break;
                    case "4": _repository.DisplayList();
                        break;
                    case "5": EmailStopApplication();
                        break;
                    default: ManagerTaskItem();
                        break;
                }
            }
            else
            {
                _repository.ConsoleText("No cuenta con tareas");
                TaskManagerByAccion(Accion.Nuevo);
            }
        }
    }

    private void TaskManagerByAccion(Accion accion)
    {

        string title = String.Empty;
        string desc = String.Empty;
        string id = string.Empty;
        int hora = 0, valorid = 0;

        if (accion != Accion.Nuevo)
        {
            _repository.DisplayList();
            string descrip = accion == Accion.Editar ? "editar" : "eliminar";
            Console.WriteLine();
            _repository.NormalConsoleText($"Id de tarea que desea {descrip}: ");
            id = Console.ReadLine().Trim();

            try
            {
                valorid = int.Parse(id);
                if (!_repository.ElementIsValid(valorid))
                {                    
                    throw new Exception();
                }
            }
            catch
            {
                _repository.ConsoleText("Valor de Id " + id + " no existe, itente con un valor valido");
                System.Threading.Thread.Sleep(4000);
                ManagerTaskItem();
            }
        }
        
        if (accion != Accion.Eliminar)
        {
            Console.WriteLine();
            _repository.NormalConsoleText("Nuevo titulo de tarea: ");
            title = Console.ReadLine().Trim();
            _repository.NormalConsoleText("Nueva hora de tarea: ");
            string horastr = Console.ReadLine().Trim();

            try
            {                
               hora = int.Parse(horastr);
            }
            catch
            {
                _repository.ConsoleText("Hora no valida.\nLa hora de la tarea, sera igual a al actual.\n");
                hora = DateTime.Now.Hour;
            }
            
            
            _repository.NormalConsoleText("Nueva descripción de tarea: ");
            desc = Console.ReadLine();
        }

        AccionTask(accion, title, desc, valorid, hora);       
    }

    private void AccionTask(Accion accion, string title, string desc, int id, int hora)
    {
        _repository.ConsoleText("\nProcesando...");
        if (accion == Accion.Nuevo)
        {
            _repository.Add(title, desc, hora);        
        }
        else
        {
            if (accion == Accion.Editar)
            {
                _repository.EditEntry(id, title, desc, hora);
            }
            else if (accion == Accion.Eliminar)
            {
                _repository.DeleteEntry(id);
            }
        }
    }

    private void EmailStopApplication()
    {
        if (!string.IsNullOrEmpty(correoelectronico))
        {
            string mensaje = "Gracias por utilizar los servicios de Veloz TODO";
            string mensajevalidation = FormatTaskHtml(correoelectronico, true);
            if (mensajevalidation.Length > 0)
            {
                _repository.ConsoleText("Verificando para enviar reporte del día...");    
                Email(correoelectronico, mensaje, mensajevalidation);
            }                  
        }        
     
       Environment.Exit(0);
    }

    public void EmailNotifyTask()
    {
        _repository.WelComsoleVelozTODO();
        _repository.ConsoleText("\n¿Tienes correo electronico? (s) para confirmar y (cualquier otra letra diferente de s) para saltar la pregunta");
        string result = Console.ReadLine();
        if(result.ToLower().Equals("s"))
        {
            _repository.ConsoleText("El correo electronico, es para notificar hacerca de las tareas que vaya agregando, y el estado.");
            _repository.ConsoleText("\nEscriba su correo electronico");
            correoelectronico = Console.ReadLine();
            if(IsValidEmail(correoelectronico))
            {
                _repository.ConsoleText("Veríficando...");
                string mensaje = "Tarea(s) pendientes de chequeo.";            
                string mensajevalidation = FormatTaskHtml(correoelectronico);
                if(mensajevalidation.Length > 0)
                {
                    Email(correoelectronico, mensaje, mensajevalidation);
                }                
            }
            else
            {
                _repository.ConsoleText("\nCorreo electronico no valido");
                EmailNotifyTask();
            }
        }
        Console.Clear();
    }

    private string FormatTaskHtml(string correo, bool afterclose = false)
    {
        StringBuilder builder = new StringBuilder();
        bool hastask = false;
        foreach(DataRowView item in _repository.GetTareas())
        {
            int hora = int.Parse(item["Hora"].ToString());

            if(hora <= DateTime.Now.Hour || afterclose)
            {
                if(hastask == false)
                {
                    builder.Append("Hola " + correo + " gracias por utilizar Veloz TODO");
                    builder.Append("Tarea(s) pendientes de chequeo:");
                }

                string horaformat = _repository.TimeFormat(item["Hora"].ToString());               

                builder.Append("<ul>"+
                     $"<li>Id: {item["Id"].ToString()}</li>"+
                     $"<li>Titulo: {item["Titulo"].ToString()}</li>"+
                     $"<li>Hora: {horaformat}</li>" +
                     $"<li>Descripción: {item["Descripcion"].ToString()}</li>"+
                    "</ul>");

                hastask = true;
            }
        }

        return builder.ToString();
    }

    private void Email(string email, string mensaje, string mensajebody)
    {
        MailAddress to = new MailAddress(email);
        MailAddress from = new MailAddress(EmailHost);
        
        MailMessage mail = new MailMessage(from, to);

        try
        {
            mail.IsBodyHtml = true;
            mail.Subject = mensaje;
            mail.Body = mensajebody;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;

            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(EmailHost, PasswordHost);
            //smtp.TargetName
            _repository.ConsoleText("Enviando correo...");
            smtp.Send(mail);
        }
        catch (Exception ex)
        {            
            Console.WriteLine(ex.Message);
        }
        finally
        {
            mail.Dispose();
        }
    }

    private bool IsValidEmail(string strIn)
    {
        // Return true if strIn is in valid e-mail format.
        return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\
        .[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    }   
}
