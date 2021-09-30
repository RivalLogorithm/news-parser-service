namespace ParserApp
{
    partial class Installer1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            processInstaller = new System.ServiceProcess.ServiceProcessInstaller();

            processInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "ParserApp";
            serviceInstaller.DisplayName = "Парсер записей с ria.ru";
            Installers.AddRange(new System.Configuration.Install.Installer[]
                { this.processInstaller, this.serviceInstaller});
        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller processInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;
    }
}