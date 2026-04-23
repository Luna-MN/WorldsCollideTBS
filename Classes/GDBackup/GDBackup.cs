using Godot;
using System;
[Tool]
[GlobalClass]
public partial class GDBackup : Resource
{
    public Resource Backup;
    private GodotObject parent;
    private string backupPath;
    private string templatePath;
    private Resource template;
    public GDBackup() { }
    
    /* name
     * class_name
     * type
     * hint
     * hint_string
     * usage
     */
    
    public GDBackup(GodotObject parent, string BackupPath, string TemplatePath)
    {
        this.parent = parent;
        backupPath = BackupPath;
        templatePath = TemplatePath;
    }
    public GDBackup(GodotObject parent, string BackupPath)
    {
        this.parent = parent;
        backupPath = BackupPath;
    }
    private void Save()
    {
        if (templatePath == null)
        {
            GD.Print("No template path");
        }
        try
        {
            Backup = GD.Load(backupPath);
            if (Backup == null)
            {
                template = GD.Load(templatePath);
                Backup = template.Duplicate();
            }

            var propertyList = parent.GetPropertyList();
            foreach (var prop in propertyList)
            {
                var name = prop["name"].ToString();
                var value = parent.Get(prop["name"].ToString());
                try
                {
                    if (value.ToString() == "" || value.ToString() == "0" || name == "script" || name.ToLower().Contains("resource"))
                    {
                        continue;
                    }
                    Backup.Set(name, value);
                }
                catch
                {
                    // ignored
                }
            }
            ResourceSaver.Save(Backup, backupPath);
        }
        finally
        {
            if (Backup != null)
            {
                Backup.Dispose();
                Backup = null;
            }
            if (template != null)
            {
                template.Dispose();
                template = null;
            }
        }
    }
    public bool Load()
    {
        try
        {
            Backup = GD.Load(backupPath);
            if (Backup == null)
            {
                return false;
            }
            var propertyList = parent.GetPropertyList();
            foreach (var prop in propertyList)
            {
                var name = prop["name"].ToString();
                var value = Backup.Get(name);
                try
                {
                    if (value.ToString() == "" || value.ToString() == "0" || name == "script" || name.ToLower().Contains("resource"))
                    {
                        continue;
                    }
                    parent.Set(name, value);
                }
                catch
                {
                    // ignored
                }
            }
            return true;
        }
        finally
        {
            if (Backup != null)
            {
                Backup.Dispose();
                Backup = null;   
            }
        }
    }
    public void LoadOrSave()
    {
        var loaded = Load();
        if (!loaded)
        {
            Save();
        }
    }
}
