using System;

namespace TodoList
{
    class Program
    {
        static void Main(string[] args)
        {
            //Disable button close app Veloz TODO
            //ApiAplication.DisableCloseMenu();

            const string path = "TODO.xml";

            var service = new TaskService(path);
            service.EmailNotifyTask();
            service.ManagerTaskItem();
        }
    }
}
