#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PuebloLunariaFinish
{
    const string ScenePath="Assets/PuebloLunaria_Blockout_Exact.unity";
    const string BackupDir="Assets/Game/Scenes/Levels/RecoveryBackups";

    [MenuItem("Tools/Kingdom Shadow/Finish Current Pueblo de Lunaria")]
    public static void Run()
    {
        if(!File.Exists(ScenePath)){EditorUtility.DisplayDialog("Lunaria","No se encontró "+ScenePath,"OK");return;}
        if(!EditorUtility.DisplayDialog("Completar Lunaria","Se hará un respaldo y se completará la escena manual actual sin tocar la geometría jugable.","Completar","Cancelar"))return;
        EnsureFolder("Assets/Game/Scenes"); EnsureFolder("Assets/Game/Scenes/Levels"); EnsureFolder(BackupDir);
        string backup=BackupDir+"/PuebloLunaria_before_finish_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";
        AssetDatabase.CopyAsset(ScenePath,backup);
        var scene=EditorSceneManager.OpenScene(ScenePath);
        var root=GameObject.Find("SCN_PuebloLunaria");
        if(root==null){EditorUtility.DisplayDialog("Lunaria","No existe SCN_PuebloLunaria","OK");return;}
        var bg=Child(root.transform,"Visual_Background");
        var fg=Child(root.transform,"Visual_Foreground");
        var lv=Child(root.transform,"Lighting_VFX");

        FinishHouseA(bg);
        FinishHouseB(bg);
        FinishMarket(bg);
        FinishBridge(bg);
        FinishFaroles(bg,lv);
        FinishScenery(bg,fg);
        FinishLighting(lv);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Lunaria","Escena completada. Backup:\n"+backup,"OK");
    }

    static void FinishHouseA(Transform bg){
        var h=Child(bg,"CASA-A"); h.localPosition=new Vector3(14,4,7); var art=Child(h,"ART_Blockout"); var t=Child(art,"Timber_Frame");
        Cube("Beam_Gable_Center",t,new Vector3(0,2.25f,-2.84f),new Vector3(.22f,2.3f,.16f),0);
        Cube("Beam_Gable_Left",t,new Vector3(-1.7f,2.25f,-2.84f),new Vector3(3.9f,.18f,.16f),31);
        Cube("Beam_Gable_Right",t,new Vector3(1.7f,2.25f,-2.84f),new Vector3(3.9f,.18f,.16f),-31);
        Cube("Door",art,new Vector3(-1.7f,-2.55f,-2.78f),new Vector3(1.35f,2.5f,.18f),0);
        Cube("Window_Lower",art,new Vector3(1.55f,-2.3f,-2.78f),new Vector3(1.4f,1.2f,.16f),0);
        Cube("Window_Upper_Left",art,new Vector3(-1.95f,.15f,-2.82f),new Vector3(1.25f,1.05f,.15f),0);
        Cube("Window_Upper_Right",art,new Vector3(1.95f,.15f,-2.82f),new Vector3(1.25f,1.05f,.15f),0);
        Cube("Stone_Base",art,new Vector3(0,-3.82f,-2.75f),new Vector3(8.5f,.35f,.35f),0);
    }

    static void FinishHouseB(Transform bg){
        var h=Child(bg,"CASA-B"); h.localPosition=new Vector3(37,5,8); var a=Child(h,"ART_Blockout");
        Cube("Body_Lower",a,new Vector3(0,-2.9f,0),new Vector3(9.8f,3.2f,6.2f),0);
        Cube("Body_Upper",a,new Vector3(0,0,0),new Vector3(10.6f,2.8f,6f),0);
        Cube("Roof_Left",a,new Vector3(-2.55f,2.85f,0),new Vector3(6.35f,.45f,7.2f),34);
        Cube("Roof_Right",a,new Vector3(2.55f,2.85f,0),new Vector3(6.35f,.45f,7.2f),-34);
        Cube("Door",a,new Vector3(2.6f,-2.9f,-3.2f),new Vector3(1.5f,2.7f,.18f),0);
        Cube("Window_Lower_Left",a,new Vector3(-2.5f,-2.6f,-3.2f),new Vector3(1.6f,1.3f,.16f),0);
        Cube("Window_Upper_Left",a,new Vector3(-2.5f,.05f,-3.18f),new Vector3(1.45f,1.15f,.16f),0);
        Cube("Window_Upper_Right",a,new Vector3(2.5f,.05f,-3.18f),new Vector3(1.45f,1.15f,.16f),0);
    }

    static void FinishMarket(Transform bg){
        var m=Child(bg,"MERCADO"); m.localPosition=new Vector3(49,3.5f,5.5f); var a=Child(m,"ART_Blockout");
        Cube("Counter",a,new Vector3(0,-1.55f,-1.15f),new Vector3(6.5f,1.1f,1.1f),0);
        Cube("Post_Left",a,new Vector3(-3.35f,-.1f,-.7f),new Vector3(.25f,3.8f,.25f),0);
        Cube("Post_Right",a,new Vector3(3.35f,-.1f,-.7f),new Vector3(.25f,3.8f,.25f),0);
        Cube("Canopy",a,new Vector3(0,1.65f,-.25f),new Vector3(7.6f,.25f,3.2f),-4);
        Cube("Crate_01",a,new Vector3(-3.9f,-1.75f,-1.25f),new Vector3(1.1f,1f,1f),0);
        Cube("Crate_02",a,new Vector3(3.85f,-1.8f,-1f),new Vector3(.9f,.9f,.9f),0);
    }

    static void FinishBridge(Transform bg){
        var b=Child(bg,"PUENTE"); b.localPosition=new Vector3(81,.5f,7); var a=Child(b,"ART_Blockout");
        Cube("Deck",a,new Vector3(0,1.4f,0),new Vector3(14,.55f,5),0);
        Cube("Pier_Left",a,new Vector3(-5.1f,-.2f,0),new Vector3(2.1f,3.2f,5),0);
        Cube("Pier_Right",a,new Vector3(5.1f,-.2f,0),new Vector3(2.1f,3.2f,5),0);
        Cube("Arch_Header",a,new Vector3(0,.65f,0),new Vector3(8.1f,.8f,5),0);
        Cube("Rail_Front",a,new Vector3(0,2.05f,-2.35f),new Vector3(14,.3f,.3f),0);
        Cube("Rail_Back",a,new Vector3(0,2.05f,2.35f),new Vector3(14,.3f,.3f),0);
    }

    static void FinishFaroles(Transform bg,Transform lv){
        var g=Child(bg,"FAROLES"); float[] x={8,30,55,68,92}; float[] y={0,1.5f,1.5f,3,3};
        for(int i=0;i<5;i++){var f=Child(g,"FAROL-0"+(i+1)); f.localPosition=new Vector3(x[i],y[i],2.8f);
            Cyl("Pole",f,new Vector3(0,1.55f,0),new Vector3(.12f,1.55f,.12f));
            Cube("Arm",f,new Vector3(-.35f,2.85f,-.02f),new Vector3(.8f,.12f,.12f),10);
            Cube("Lamp",f,new Vector3(-.7f,2.55f,-.02f),new Vector3(.45f,.6f,.45f),0);
            var lt=Child(lv,"LampLight_0"+(i+1)); lt.position=f.TransformPoint(new Vector3(-.7f,2.55f,-.02f));
            var l=lt.GetComponent<Light>()??lt.gameObject.AddComponent<Light>(); l.type=LightType.Point; l.range=5.5f; l.intensity=1.6f; l.color=new Color(1,.72f,.38f);
        }
    }

    static void FinishScenery(Transform bg,Transform fg){
        var s=Child(bg,"SCENIC_Background");
        Tree("Tree_Entrance",s,new Vector3(2,1.5f,9.5f),.95f);
        Tree("Tree_Market",s,new Vector3(57,2.7f,10.5f),.85f);
        Tree("Tree_Bridge",s,new Vector3(90,3,11),1.25f);
        var f=Child(fg,"SCENIC_Foreground");
        Sphere("Bush_FG_01",f,new Vector3(18,.45f,-4.8f),new Vector3(1.8f,.9f,.9f));
        Sphere("Bush_FG_02",f,new Vector3(52,1.9f,-5),new Vector3(2.2f,1.1f,1));
        Sphere("Bush_FG_03",f,new Vector3(87,3.45f,-5.2f),new Vector3(2,1,1));
    }

    static void FinishLighting(Transform lv){
        var t=Child(lv,"Directional Light_Lunaria"); t.localRotation=Quaternion.Euler(42,-32,0);
        var l=t.GetComponent<Light>()??t.gameObject.AddComponent<Light>(); l.type=LightType.Directional; l.intensity=1.15f; l.shadows=LightShadows.Soft;
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.16f,.18f,.22f); RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogStartDistance=35; RenderSettings.fogEndDistance=95;
    }

    static Transform Child(Transform p,string n){var t=Find(p,n); if(t)return t; var g=new GameObject(n); g.transform.SetParent(p,false); return g.transform;}
    static Transform Find(Transform p,string n){for(int i=0;i<p.childCount;i++)if(p.GetChild(i).name==n)return p.GetChild(i);return null;}
    static void Cube(string n,Transform p,Vector3 pos,Vector3 scale,float rz){var t=Find(p,n);var g=t?t.gameObject:GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;if(!t)g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.transform.localRotation=Quaternion.Euler(0,0,rz);RemoveCol(g);}
    static void Cyl(string n,Transform p,Vector3 pos,Vector3 scale){var t=Find(p,n);var g=t?t.gameObject:GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;if(!t)g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;RemoveCol(g);}
    static void Sphere(string n,Transform p,Vector3 pos,Vector3 scale){var t=Find(p,n);var g=t?t.gameObject:GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=n;if(!t)g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;RemoveCol(g);}
    static void Tree(string n,Transform p,Vector3 pos,float s){var t=Child(p,n);t.localPosition=pos;t.localScale=Vector3.one*s;Cyl("Trunk",t,new Vector3(0,2,0),new Vector3(.45f,2,.45f));Sphere("Canopy",t,new Vector3(0,5,0),new Vector3(3.2f,2.4f,2.3f));}
    static void RemoveCol(GameObject g){var c=g.GetComponent<Collider>(); if(c) UnityEngine.Object.DestroyImmediate(c);}
    static void EnsureFolder(string path){if(AssetDatabase.IsValidFolder(path))return;string parent=Path.GetDirectoryName(path)?.Replace('\\','/');string name=Path.GetFileName(path);if(string.IsNullOrEmpty(parent))return;EnsureFolder(parent);AssetDatabase.CreateFolder(parent,name);}
}
#endif