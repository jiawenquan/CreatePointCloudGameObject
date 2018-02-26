using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Unity Point Cloud Loader
// (C) 2016 Ryan Theriot, Eric Wu, Jack Lam. Laboratory for Advanced Visualization & Applications, University of Hawaii at Manoa.
// Version: February 17th, 2017

public class PointCloudLoaderWindow : EditorWindow
{
    //Number of elements per data line from input file
    //每一行中 共有有几个数
    private static int elementsPerLine = 0;

    //Position of XYZ and RGB elements in data line
    //数据行中的XYZ和RGB元素的位置 所对应指定的索引
    private static int rPOS, gPOS, bPOS, xPOS, yPOS, zPOS;

    //Enumerator for PointCloud color range
    //点云的枚举器
    //None = No Color, Normalized = 0-1.0f, RGB = 0-255
    private enum ColorRange
    {
        NONE = 0,
        NORMALIZED = 1,
        RGB = 255
    }

    //颜色范围枚举
    private static ColorRange colorRange;

    //Enumber for format standards
    //Enumber格式标准  
    private enum FormatStandard
    {
        CUSTOM = 0,
        PTS = 1,
        XYZ = 2,
        XYZRGB = 3
    }

    //用来规定加载数据的格式标准
    private static FormatStandard formatStandard;


    //Data line delimiter  
    //数据行分隔符 默认空格
    public static string dataDelimiter;

    //Maximum vertices a mesh can have in Unity
    //一个网格的最大顶点可以在unity中
    static int limitPoints = 65000;  

    [MenuItem("Window/PointClouds/加载点云")]  //LoadCloud
    private static void ShowEditor()
    {
        //创建一个窗体
        EditorWindow window = GetWindow(typeof(PointCloudLoaderWindow), true, "Point Cload Loader");
        window.maxSize = new Vector2(485f, 475f); //窗体的最大尺寸 385f, 375f
        window.minSize = window.maxSize;          //窗体的最小尺寸
    }

    //GUI Window Stuff - NO COMMENTS
    private void OnGUI()
    {

        //创建一个提示信息 label
        GUIStyle help = new GUIStyle(GUI.skin.label);
        help.fontSize = 12;
        help.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("如何使用", help);

        EditorGUILayout.HelpBox("1. 设置每个数据行中存在的元素数量. \n" +
                                "2. 在每个元素之间设置分隔符. (默认的为空格分割) \n" +
                                "3. 在数据行上设置XYZ元素的索引。. (第一个索引=1) \n" +
                                "4. 选择颜色数据的范围: \n" +
                                "       None: 没有颜色数据 \n" +
                                "       Normalized: 0.0 - 1.0 \n" +
                                "       RGB : 0 - 255 \n" +
                                "5. 在数据线上设置RGB元素的索引. \n" +
                                "6. Click \"Load Point Cloud File\"", MessageType.None);


        formatStandard = (FormatStandard)EditorGUILayout.EnumPopup(new GUIContent("Format", ""), formatStandard); //创建一个枚举选项

        if (formatStandard == FormatStandard.CUSTOM) //默认自定义数据索引
        {
            EditorGUILayout.BeginHorizontal();       //绘制一个水平布局

            EditorGUILayout.BeginVertical();         //绘制一个垂直布局

            elementsPerLine = 6;
            dataDelimiter = "";
            xPOS = 1;
            yPOS = 2;
            zPOS = 3;
            colorRange = ColorRange.RGB;  //颜色区间
            rPOS = 4;
            gPOS = 5;
            bPOS = 6;


            elementsPerLine = EditorGUILayout.IntField(new GUIContent("Elements Per Data Line", "数据行中元素的数量\nThe Number of Elements in the data line"), elementsPerLine);
            dataDelimiter = EditorGUILayout.TextField(new GUIContent("Data Line Delimiter", "在元素之间留空空白"), dataDelimiter);
            xPOS = EditorGUILayout.IntField(new GUIContent("X Index", "X值的索引"), xPOS);
            yPOS = EditorGUILayout.IntField(new GUIContent("Y Index", "Y值的索引"), yPOS);
            zPOS = EditorGUILayout.IntField(new GUIContent("Z Index", "Z值的索引"), zPOS);

            colorRange = (ColorRange)EditorGUILayout.EnumPopup(new GUIContent("Color Range", "None(No Color), Normalized (0.0-1.0f), RGB(0-255)"), colorRange);

            if (colorRange == ColorRange.NORMALIZED || colorRange == ColorRange.RGB)
            {
                rPOS = EditorGUILayout.IntField(new GUIContent("Red Index", "R值的索引"), rPOS);
                gPOS = EditorGUILayout.IntField(new GUIContent("Green Index", "G值的索引"), gPOS);
                bPOS = EditorGUILayout.IntField(new GUIContent("Blue Index", "B值的索引"), bPOS);
            }
            EditorGUILayout.EndVertical();     //关闭绘制
             
            EditorGUILayout.EndHorizontal();   //关闭绘制
        }
        else if (formatStandard == FormatStandard.PTS)
        {
            //PTS 的数据 如果每行7个数 第四个数没用  前三个数是坐标 后三个是颜色
            elementsPerLine = 7;
            dataDelimiter = "";
            xPOS = 1;
            yPOS = 2;
            zPOS = 3;
            colorRange = ColorRange.RGB;
            rPOS = 5;
            gPOS = 6;
            bPOS = 7;
        }
        else if (formatStandard == FormatStandard.XYZ) //只加载点的坐标
        {
            elementsPerLine = 3;
            dataDelimiter = "";
            xPOS = 1;
            yPOS = 2;
            zPOS = 3;
            colorRange = ColorRange.NONE;  //无颜色
        }
        else if (formatStandard == FormatStandard.XYZRGB)
        {
            elementsPerLine = 6;
            dataDelimiter = "";
            xPOS = 1;
            yPOS = 2;
            zPOS = 3;
            colorRange = ColorRange.NORMALIZED;
            rPOS = 4;
            gPOS = 5;
            bPOS = 6;
        }

        //空格隔出一点空间
        EditorGUILayout.Space();

        //创建一个按钮
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Load Point Cloud File", buttonStyle, GUILayout.Height(50)))
        {
            LoadCloud();//调用加载的方法
        }

    }


    private void LoadCloud()
    {
        //Get path to file with EditorUtility
        //打开文件对话框 选中 获取到文件路径
        string path = EditorUtility.OpenFilePanel("Load Point Cloud File", "", "*");

        //If path doesn't exist of user exits dialog exit function
        // 如果路径不存在用户退出对话框退出函数
        if (path.Length == 0) return;  

        //Set data delimiter  
        //设置数据的分割符
        char delimiter = ' ';
        try
        {
            
            if (dataDelimiter.Length != 0)
            {
                
                delimiter = dataDelimiter.ToCharArray()[0];
            }
        }
        catch (NullReferenceException)
        {
        }

        //Create string to name future asset creation from file's name
        //创建字符串，以从文件名中命名 未来将要创建的文件资源
        string filename = null;
        try
        {
            filename = Path.GetFileName(path).Split('.')[0];
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError("PointCloudLoader: File must have an extension. (.pts, .xyz....etc)" + e);
        }

        //Create PointCloud Directories
        //创建PointCloud目录
        if (!Directory.Exists(Application.dataPath + "/PointClouds/"))
        {
            
            AssetDatabase.CreateFolder("Assets", "PointClouds");
        }

        //创建点云名字命名的文件夹目录用来存放 解析得到的所有文件
        if (!Directory.Exists(Application.dataPath + "/PointClouds/" + filename))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets/PointClouds", filename);
        }


        //Setup Progress Bar 
        //设置进度条
        float progress = 0.0f; 
        EditorUtility.ClearProgressBar(); //删除进度条

        //显示或更新进度条。
        EditorUtility.DisplayProgressBar("Progress", "Percent Complete: " + (int)(progress * 100) + "%", progress);

        //Setup variables so we can use them to center the PointCloud at origin
        //设置变量，这样我们就可以使用它们来在原点处居中
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;
        float zMin = float.MaxValue;
        float zMax = float.MinValue;

        //Streamreader to read data file
        //读取数据文件的流读取器
        StreamReader sr = new StreamReader(path);
        string line;

        //Could use a while loop but then cant show progress bar progression
        //可以使用while循环，但不能显示进度条进度  用 while 获取行数的速度应该更快 这里用来是为了更简便易懂
        int numberOfLines = File.ReadAllLines(path).Length;  //总行数
        
        int numPoints = 0;

        //For loop to count the number of data points (checks again elementsPerLine which is set by user)
        //For循环计算数据点的数量(再次检查由用户设置的elementsPerLine)
        //Calculates the min and max of all axis to center point cloud at origin
        //计算所有轴的最小值和最大值点到中心点云的原点 

        //这里的循环仅仅是为了得到点云点 的哥哥坐标的最大值 与最小值  为了下面的创建点云物体的远点
        for (int i = 0; i < numberOfLines; i++)
        {
            line = sr.ReadLine();
            string[] words = line.Split(delimiter); //分割成字符串数组

            //Only read data lines
            //
            if (words.Length == elementsPerLine)
            {
                numPoints++;

                if (xMin > float.Parse(words[xPOS - 1]))
                    xMin = float.Parse(words[xPOS - 1]);

                if (xMax < float.Parse(words[xPOS - 1]))
                    xMax = float.Parse(words[xPOS - 1]);

                if (yMin > float.Parse(words[yPOS - 1]))
                    yMin = float.Parse(words[yPOS - 1]);
                if (yMax < float.Parse(words[yPOS - 1]))
                    yMax = float.Parse(words[yPOS - 1]);

                if (zMin > float.Parse(words[zPOS - 1]))
                    zMin = float.Parse(words[zPOS - 1]);
                if (zMax < float.Parse(words[zPOS - 1]))
                    zMax = float.Parse(words[zPOS - 1]);

            }
            else
            {
                //Debug.LogError("Elements Per Data Line 错误 请修改每行元素个数"); 
            }

            //Update progress bar -Only updates every 10,000 lines - DisplayProgressBar is not efficient and slows progress
            //每解析一万行进度条更新一次
            progress = i * 1.0f / numberOfLines * 1.0f;
            if (i % 10000 == 0)
            {
                EditorUtility.DisplayProgressBar("计算原点Progress", "Percent Complete: " + (int)((progress * 100) / 3) + "%", progress / 3);
            }
                

        }

        //Calculate origin of point cloud to shift cloud to unity origin
        //计算点云的原点，将云移到统一原点
        float xAvg = (xMin + xMax) / 2;
        float yAvg = (yMin + yMax) / 2;
        float zAvg = (zMin + zMax) / 2;

        //Setup array for the points and their colors
        //为这些点和它们的颜色设置数组
        Vector3[] points = new Vector3[numPoints]; //储存顶点
        Color[] colors = new Color[numPoints];     //储存顶点颜色

        //Reset Streamreader 读取文件流
        sr = new StreamReader(path);

        //For loop to create all the new vectors from the data points
        //For循环从数据点创建所有新矢量
        for (int i = 0; i < numPoints; i++)
        {
            line = sr.ReadLine();
            string[] words = line.Split(delimiter); //分割每行得到字符串数组

            //Only read data lines
            //只有读取数据行
            while (words.Length != elementsPerLine) //判断每行分割的数是否为 规定的每行元素个数  如果不是读取下一行 如果下一行相匹配 跳出循环
            {
                line = sr.ReadLine();
                words = line.Split(delimiter);       
            }

            //Read data line for XYZ and RGB
            //读取XYZ和RGB的数据行
            float x = float.Parse(words[xPOS - 1]) - xAvg; //设置x的坐标
            float y = float.Parse(words[yPOS - 1]) - yAvg; //设置y的坐标
            float z = (float.Parse(words[zPOS - 1]) - zAvg)* -1; //设置z的坐标  Flips to Unity's Left Handed Coorindate System
            float r = 1.0f;
            float g = 1.0f;
            float b = 1.0f;

            //If color range has been set also get color from data line
            if (colorRange == ColorRange.NORMALIZED || colorRange == ColorRange.RGB)
            {
                r = float.Parse(words[rPOS - 1]) / (int)colorRange;   //获取到标准单位的R值
                g = float.Parse(words[gPOS - 1]) / (int)colorRange;   //获取到标准单位的G值
                b = float.Parse(words[bPOS - 1]) / (int)colorRange;   //获取到标准单位的B值
            }

            //Save new vector to point array
            //Save new color to color array
            points[i] = new Vector3(x, y, z);    //保存点的坐标
            colors[i] = new Color(r, g, b, 1.0f);//保存点的颜色

            //Update Progress Bar
            progress = i * 1.0f / (numPoints - 1) * 1.0f;
            if (i % 10000 == 0)
            {
                EditorUtility.DisplayProgressBar("创建点云Progress", "Percent Complete: " + (int)(((progress * 100) / 3) + 33) + "%", progress / 3 + .33f);
            }
                


        }

        //Close Stream reader
        sr.Close(); //关闭读取流


        // Instantiate Point Groups
        //Unity limits the number of points per mesh to 65,000.  
        //For large point clouds the complete mesh wil be broken down into smaller meshes
        int numMeshes = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);  //要创建几组网格

        //Create the new gameobject
        //创建一个物体
        GameObject cloudGameObject = new GameObject(filename); 

        //Create an new material using the point cloud shader
        //创建一个与这个物体相匹配的材质
        Material newMat = new Material(Shader.Find("PointCloudShader"));
        //Save new Material
        //保存材质
        AssetDatabase.CreateAsset(newMat, "Assets/PointClouds/" + filename + "Material" + ".mat");

        //Create the sub meshes of the point cloud
        //创建点云的子网格
        for (int i = 0; i < numMeshes - 1; i++)
        {
            CreateMeshGroup(i, limitPoints, filename, cloudGameObject, points, colors, newMat);

            progress = i * 1.0f / (numMeshes - 2) * 1.0f;
            if (i % 2 == 0)
            {
                EditorUtility.DisplayProgressBar("创建点云网格Progress", "Percent Complete: " + (int)(((progress * 100) / 3) + 66) + "%", progress / 3 + .66f);
            }
                

        }
        //Create one last mesh from the remaining points
        //从剩余的点创建最后一个网格
        int remainPoints = (numMeshes - 1) * limitPoints;
        CreateMeshGroup(numMeshes - 1, numPoints - remainPoints, filename, cloudGameObject, points, colors, newMat);

        progress = 100.0f;
        EditorUtility.DisplayProgressBar("创建点云网格Progress", "Percent Complete: " + progress + "%", 1.0f);

        //Store PointCloud
        //创建预制体 保存点云物体
        UnityEditor.PrefabUtility.CreatePrefab("Assets/PointClouds/" + filename + ".prefab", cloudGameObject);
        EditorUtility.ClearProgressBar();  //关闭进度条
        //EditorUtility.DisplayDialog("Point Cloud Loader", filename + " Saved to PointClouds folder", "Continue", "");
        EditorUtility.DisplayDialog("Point Cloud Loader","成功创建并保存点云"+"\""+filename + "\"", "继续","");

        return;
    }

    //创建网格
    private void CreateMeshGroup(int meshIndex, int numPoints, string filename, GameObject pointCloud, Vector3[] points, Color[] colors, Material mat)
    {

        //Create GameObject and set parent
        GameObject pointGroup = new GameObject(filename + meshIndex); //创建子物体点云网格
        pointGroup.transform.parent = pointCloud.transform;           //成为pointCloud 子物体

        //Add mesh to gameobject
        //添加一个网格对象
        Mesh mesh = new Mesh();     //创建Mesh网格
        pointGroup.AddComponent<MeshFilter>(); //添加MeshFilter 网格组建
        pointGroup.GetComponent<MeshFilter>().mesh = mesh;  //赋值网格

        //Add Mesh Renderer and material
        //添加一个网格渲染器 与材质
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().sharedMaterial = mat;

        //Create points and color arrays
        //创建点和颜色数组
        int[] indecies = new int[numPoints];
        Vector3[] meshPoints = new Vector3[numPoints];
        Color[] meshColors = new Color[numPoints];

        for (int i = 0; i < numPoints; ++i)
        {
            indecies[i] = i;
            meshPoints[i] = points[meshIndex * limitPoints + i];  //得到网格顶点坐标数组
            meshColors[i] = colors[meshIndex * limitPoints + i];  //得到网格顶点颜色坐标的数组
        }

        //Set all points and colors on mesh
        //在网格上设置所有的点和颜色
        mesh.vertices = meshPoints;  
        mesh.colors = meshColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);

        //Create bogus uv and normals
        //创建假uv和法线
        mesh.uv = new Vector2[numPoints];
        mesh.normals = new Vector3[numPoints];

        // Store Mesh 
        UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/PointClouds/" + filename + @"/" + filename + meshIndex + ".asset"); //创建.asset文件储存网格数据
        UnityEditor.AssetDatabase.SaveAssets(); //保存.asset网格文件
        UnityEditor.AssetDatabase.Refresh();
        return;
    }

}