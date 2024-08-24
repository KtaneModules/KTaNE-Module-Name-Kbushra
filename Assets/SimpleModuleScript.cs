using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	//Base vars
	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable[] selectables;
	public KMSelectable[] moduleSelect;
	public TextMesh[] Displays;
	public GameObject Screen;
	static int ModuleIdCounter = 1;
	int ModuleId;

	//Main vars
	private int pressCount;
	long output;
	char[] outputchar;
	int input;
	int stage = 0;
	int randCount;
	int randHTML;
	int randCol;
	string[] htmlList = new string[32]
	{"<p>1", "<tr>3", "<tr>4<td>1", "<tr>2<th>1", "<ul>1<li>3", "<ul>2<li>1<strong>1", "<h3>3", "<p>2", "<ul>3<li>3", "<p>3", "<h3>1", 
	"<ul>1<li>2", "<p>4", "<tr>1<th>1", "<tr>4<th>1", "<h3>2", "<p>6", "<ul>1<li>1", "<ul>1<li>3<ol>1", "<h4>1", "<tr>8<th>1",
	"<ul>1<li>2<ul>1", "<h3>5", "<h3>4", "<ul>3<li>1", "<ul>2<li>2<em>1", "<tr>2<td>1", "<ol>1<li>1", "<img>1", "<ul>3<li>5<ul>1", "<i>2", "<tr>9"};
	public Material[] screenCol;
	bool _isSolved = false;
	bool incorrect = false;
	public AudioSource ding;

	//Octadecayotton vars
	int[] octaList1 = new int[3]
	{2, 6, 9};
	int[] octaList2= new int[5]
	{2, 6, 9, 8, 2};
	int[] octaList3 = new int[7]
	{2, 6, 9, 8, 9, 9, 1};
	int[] octaList4 = new int[9]
	{2, 6, 9, 8, 9, 9, 5, 3, 9};

	//Blank Slate vars
	bool blankslateException = false;
	int[,] highpitchTable = new int[5, 5]
	{
		{ 16, 9, 20, 12, 64 },
		{5, 1, 12, 9, 32},
		{9, 16, 64, 20, 5},
		{64, 5, 1, 32, 9},
		{12, 64, 5, 1, 20}
	};
	int[,] hingefallsTable = new int[8,8]
	{
		{7, 1, 2, 8, 4, 6, 5, 3},
		{1, 4, 8, 3, 6, 5, 7, 2},
		{2, 7, 1, 4, 3, 8, 6, 5},
		{8, 6, 5, 7, 2, 1, 3, 4},
		{4, 5, 6, 2, 7, 3, 1, 8},
		{3, 8, 7, 5, 1, 2, 4, 6},
		{5, 3, 4, 6, 8, 7, 2, 1},
		{6, 2, 3, 1, 5, 4, 8, 7}
	};
	string[,] tapcodeTable = new string[7,5]
	{
		{"roe","law","raj","tee","not"},
		{"phi","jay","orb","mol","put"},
		{"rue","one","led","nun","pal"},
		{"ore","jug","see","pea","leg"},
		{"yes","now","ted","hen","sac"},
		{"wee","wry","mac","son","nil"},
		{"rib","yen","try","zoo","pit"}
	};
	bool tapsFound;

	//Hertz vars
	private static readonly int[,] hertzTable1 = new int[5, 5] 
	{
		{857, 938, 859, 200, 200},
		{490, 607, 206, 150,200},
		{552, 569, 466, 282, 948},
		{200, 485, 262, 308, 345},
		{200, 200, 398, 423, 332}
	};
	private static readonly int[,] hertzTable2 = new int[5, 5] 
	{
		{0, 12, 11, 13, 19},
		{9, 8, 6, 5, 2},
		{7, 3, 6, 5, 2},
		{1, 0, 1, 7, 6},
		{4, 9, 4, 3, 8}
	};
	int posx;
	int posy;
	int modify1;
	int modify2;

	//Simon's statement vars
	int curNum;
	int stageNum;
	private readonly int[,] simontableA = new int[5, 10]
	{
		{3, 3, 2, 0, 0, 3, 0, 3, 2, 2},
		{2, 2, 0, 1, 2, 3, 1, 3, 0, 0},
		{1, 3, 1, 3, 1, 3, 3, 1, 1, 3},
		{1, 0, 0, 3, 1, 2, 2, 0, 2, 2},
		{3, 1, 0, 2, 0, 2, 1, 3, 2, 3}
	}; //3 is yellow, 2 is blue, 1 is green, 0 is red
	private readonly int[,] simontableB = new int[5, 10]
	{
		{1, 2, 0, 2, 2, 1, 3, 0, 1, 0},
		{1, 1, 1, 0, 3, 1, 3, 0, 1, 1},
		{0, 0, 0, 2, 2, 3, 3, 0, 2, 2},
		{3, 1, 2, 0, 3, 0, 2, 3, 1, 3},
		{2, 3, 0, 1, 3, 0, 1, 3, 2, 2}
	};
	bool simonException = false;
	bool indFound = false;
	bool portFound = false;
	bool[] colorbools = new bool[4];
	int requiredCol = -1;

	//Not green arrows vars
	char[] letters = new char[3];
	int upstatements = 0;
	int rightstatements = 0;
	int downstatements = 0;
	int leftstatements = 0;
	int[] arraystatements = new int[4];

	private bool colourblindSupportOn;
	public TextMesh colourblindText;

	void Awake()
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in selectables)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { selectPress(pressedButton); return false; };
		}
		foreach (KMSelectable module in moduleSelect)
		{
			KMSelectable pressedModule = module;
			module.OnInteract += delegate () { selectModule(pressedModule); return false; };
		}
	}

	void Start()
	{
		randCount = Rnd.Range (2, 12);
		randHTML = Rnd.Range (0, 32);
		randCol = Rnd.Range (0, 4);
	}

	void FixedUpdate ()
	{
		if (colourblindSupportOn == true && pressCount == randCount + 1)
		{
			if (randCol == 0)
			{
				colourblindText.text = "Blue";
			}
			if (randCol == 1)
			{
				colourblindText.text = "Red";
			}
			if (randCol == 2)
			{
				colourblindText.text = "Yellow";
			}
			if (randCol == 3)
			{
				colourblindText.text = "Green";
			}
		}
		else
		{
			colourblindText.text = "";
		}

		if (simonException == true && input == output && input.ToString().ToCharArray().Length != 8) 
		{
			randCol = Rnd.Range (0, 4);
			Material material = screenCol [randCol];
			Screen.GetComponent<Renderer> ().material = material;
            colorbools = new bool[4] { false, false, false, false };
            simonException = false;
			if (screenCol [randCol] != screenCol [requiredCol]) 
			{
				Calculate ();
			}
		}
		if (screenCol[randCol] == screenCol[requiredCol] && curNum != 0) 
		{
			if (input.ToString().ToCharArray().Length > output.ToString().ToCharArray().Length)
			{
				module.HandleStrike ();
				Log ("Striked!");
				stage = 0;
				Reset ();
			}
		}
		if (curNum != 0 && input.ToString().ToCharArray().Length == output.ToString().ToCharArray().Length)
		{
			if (input != output)
			{
				module.HandleStrike ();
				Log ("Striked!");
				stage = 0;
				Reset ();
			}
		}
	}
		
	void selectModule(KMSelectable pressedModule)
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		pressedModule.AddInteractionPunch (1f);
		int modulePosition = Array.IndexOf(moduleSelect, pressedModule);

		if(_isSolved == false)
		{
			switch(modulePosition)
			{
			case 0:
				if (moduleSelect [0].gameObject.activeSelf == true)
				{
					pressCount++;
					if (pressCount == randCount)
					{
						Displays [0].gameObject.SetActive (true);
						Displays [0].text = htmlList [randHTML];
						ding.Play ();
						Debug.LogFormat ("[Module Name #{0}] The amount of times pressed is {1}", ModuleId, randCount);
					} 
					else if (pressCount == randCount + 1)
					{
						Displays [0].gameObject.SetActive (false);
						Displays [1].gameObject.SetActive (true);
						moduleSelect [0].gameObject.SetActive (false);
						Screen.transform.localPosition = new Vector3(Screen.transform.localPosition.x, Screen.transform.localPosition.y + 0.022f, Screen.transform.localPosition.z);
						foreach (KMSelectable button in selectables)
						{
							button.gameObject.transform.localPosition = new Vector3(button.gameObject.transform.localPosition.x, button.gameObject.transform.localPosition.y + 0.02f, button.gameObject.transform.localPosition.z);
						}
						Calculate ();
					}
				}
				break;
			}
		}
	}

	void Calculate()
	{
		Material material = screenCol [randCol];
		Screen.GetComponent<Renderer> ().material = material;

		if(htmlList[randHTML] == "<p>1" || htmlList[randHTML] == "<h3>3" || htmlList[randHTML] == "<tr>1<th>1" || htmlList[randHTML] == "<ul>3<li>1" || htmlList[randHTML] == "<tr>4<th>1" || htmlList[randHTML] == "<ul>1<li>3<ol>1" || htmlList[randHTML] == "<h4>1" || htmlList[randHTML] == "<tr>8<th>1")
		{
			if (randCount < 3) 
			{
				if(material == screenCol[0])
				{
					octaList1 [0] = -octaList1 [0];
					octaList1 [1] = -octaList1 [1];
				}
				for (int i = 0; i < 3; i++) 
				{
					output = output + octaList1 [i];
				}
				output = ((output % 8) + 8) % 8;
				output = long.Parse(Convert.ToString (output, 2));

				outputchar = output.ToString ("000").ToCharArray ();
				outputchar [0] = '1';
				Debug.LogFormat("[Module Name #{0}] The module chosen is The Octadecayotton and the output before gray code transformation is {1}", ModuleId, new string(outputchar));

				octaToGrey (3);
			}
			else if (randCount < 6) 
			{
				if(material == screenCol[0])
				{
					octaList2 [0] = -octaList2 [0];
					octaList2 [1] = -octaList2 [1];
					octaList2 [2] = -octaList2 [2];
					octaList2 [4] = -octaList2 [4];
				}
				if(material == screenCol[2])
				{
					octaList2 [3] = -octaList2 [3];
					octaList2 [4] = -octaList2 [4];
				}
				if(material == screenCol[3])
				{
					octaList2 [2] = -octaList2 [2];
					octaList2 [3] = -octaList2 [3];
				}
				for (int i = 0; i < 5; i++) 
				{
					output = output + octaList2 [i];
				}
				output = ((output % 32) + 32) % 32;
				output = long.Parse(Convert.ToString (output, 2));

				outputchar = output.ToString ("00000").ToCharArray ();
				outputchar [0] = '1';
				Debug.LogFormat("[Module Name #{0}] The module chosen is The Octadecayotton and the output before gray code transformation is {1}", ModuleId, new string(outputchar));

				octaToGrey (5);
			}
			else if (randCount < 9) 
			{
				if(material == screenCol[0])
				{
					octaList3 [0] = -octaList3 [0];
					octaList3 [1] = -octaList3 [1];
					octaList3 [2] = -octaList3 [2];
					octaList3 [6] = -octaList3 [6];
				}
				if(material == screenCol[1])
				{
					octaList3 [5] = -octaList3 [5];
					octaList3 [6] = -octaList3 [6];
				}
				if(material == screenCol[2])
				{
					octaList3 [3] = -octaList3 [3];
					octaList3 [5] = -octaList3 [5];
				}
				if(material == screenCol[3])
				{
					octaList3 [2] = -octaList3 [2];
					octaList3 [3] = -octaList3 [3];
					octaList3 [5] = -octaList3 [5];
					octaList3 [6] = -octaList3 [6];
				}
				for (int i = 0; i < 7; i++) 
				{
					output = output + octaList3 [i];
				}
				output = ((output % 128) + 128) % 128;

				output = long.Parse(Convert.ToString (output, 2));

				outputchar = output.ToString ("0000000").ToCharArray ();
				outputchar [0] = '1';
				Debug.LogFormat("[Module Name #{0}] The module chosen is The Octadecayotton and the output before gray code transformation is {1}", ModuleId, new string(outputchar));

				octaToGrey (7);
			}
			else 
			{
				if(material == screenCol[0])
				{
					octaList4 [0] = -octaList4 [0];
					octaList4 [1] = -octaList4 [1];
					octaList4 [2] = -octaList4 [2];
					octaList4 [8] = -octaList4 [8];
				}
				if(material == screenCol[1])
				{
					octaList4 [5] = -octaList4 [5];
					octaList4 [6] = -octaList4 [6];
					octaList4 [7] = -octaList4 [7];
					octaList4 [8] = -octaList4 [8];
				}
				if(material == screenCol[2])
				{
					octaList4 [3] = -octaList4 [3];
					octaList4 [5] = -octaList4 [5];
				}
				if(material == screenCol[3])
				{
					octaList4 [2] = -octaList4 [2];
					octaList4 [3] = -octaList4 [3];
					octaList4 [5] = -octaList4 [5];
					octaList4 [6] = -octaList4 [6];
				}
				for (int i = 0; i < 9; i++) 
				{
					output = output + octaList4 [i];
				}
				output = ((output % 512) + 512) % 512;
				output = long.Parse(Convert.ToString (output, 2));

				outputchar = output.ToString ("000000000").ToCharArray ();
				outputchar [0] = '1';
				Debug.LogFormat("[Module Name #{0}] The module chosen is The Octadecayotton and the output before gray code transformation is {1}", ModuleId, new string(outputchar));

				octaToGrey (9);
			}
		}

		if(htmlList[randHTML] == "<ul>1<li>2<ul>1" || htmlList[randHTML] == "<ul>2<li>1<strong>1" || htmlList[randHTML] == "<h3>4" || htmlList[randHTML] == "<img>1" || htmlList[randHTML] == "<ul>3<li>5<ul>1" || htmlList[randHTML] == "<i>2")
		{
			if (material == screenCol[0])
			{
				Debug.LogFormat ("[Module Name #{0}] Module chosen is Blank Slate and the section used is NOTHING INITIALLY", ModuleId);
				blankslateException = true;
				Debug.LogFormat ("[Module Name #{0}] Invalid numbers are {1}, {2}, {3} and {4}", ModuleId, (randCount + info.GetPortPlateCount ()) % 10, (randCount + info.GetBatteryCount ()) % 10, (randCount + info.GetSolvedModuleNames ().Count) % 10, (randCount + info.GetBatteryHolderCount ()) % 10);
			}
			if(material == screenCol[1])
			{
				Debug.LogFormat ("[Module Name #{0}] Module chosen is Blank Slate and the section used is HIGHPITCH SOUND", ModuleId);
				output = highpitchTable [info.GetSerialNumberNumbers ().ToArray () [1] % 5, randCount % 5];
				output = output + (randCount * 7);
				Debug.LogFormat ("[Module Name #{0}] The output is {1}", ModuleId, output);
			}
			if (material == screenCol[2]) 
			{
				Debug.LogFormat ("[Module Name #{0}] Module chosen is Blank Slate and the section used is HINGE FALLS", ModuleId);
				if((info.GetPortCount() % 8 + 1 > info.GetBatteryCount() % 8 + 1 && info.GetPortCount() % 8 + 1 < info.GetOnIndicators().Count() % 8 + 1) || (info.GetPortCount() % 8 + 1 < info.GetBatteryCount() % 8 + 1 && info.GetPortCount() % 8 + 1 > info.GetOnIndicators().Count() % 8 + 1))
				{
					output = (hingefallsTable [info.GetPortCount () % 8, hingefallsTable [info.GetPortCount () % 8, randCount % 8] % 8] * 100) + (hingefallsTable [info.GetBatteryCount () % 8, hingefallsTable [info.GetPortCount () % 8, randCount % 8] % 8] * 10) + hingefallsTable [info.GetOnIndicators().Count() % 8, hingefallsTable [info.GetPortCount () % 8, randCount % 8] % 8];
					Debug.LogFormat ("[Module Name #{0}] Port is middle", ModuleId);
				}
				else if((info.GetBatteryCount() % 8 + 1 > info.GetPortCount() % 8 + 1 && info.GetBatteryCount() % 8 + 1 < info.GetOnIndicators().Count() % 8 + 1) || (info.GetBatteryCount() % 8 + 1 < info.GetPortCount() % 8 + 1 && info.GetBatteryCount() % 8 + 1 > info.GetOnIndicators().Count() % 8 + 1))
				{
					output = (hingefallsTable [info.GetPortCount () % 8, hingefallsTable [info.GetBatteryCount () % 8, randCount % 8] % 8] * 100) + (hingefallsTable [info.GetBatteryCount () % 8, hingefallsTable [info.GetBatteryCount () % 8, randCount % 8] % 8] * 10) + hingefallsTable [info.GetOnIndicators().Count() % 8, hingefallsTable [info.GetBatteryCount () % 8, randCount % 8] % 8];
					Debug.LogFormat ("[Module Name #{0}] Battery is middle", ModuleId);
				}
				else if((info.GetOnIndicators().Count() % 8 + 1 > info.GetPortCount() % 8 + 1 && info.GetOnIndicators().Count() % 8 + 1 < info.GetBatteryCount() % 8 + 1) || (info.GetOnIndicators().Count() % 8 + 1 < info.GetPortCount() % 8 + 1 && info.GetOnIndicators().Count() % 8 + 1 > info.GetBatteryCount() % 8 + 1))
				{
					output = (hingefallsTable [info.GetPortCount () % 8, hingefallsTable [info.GetOnIndicators().Count() % 8, randCount % 8] % 8] * 100) + (hingefallsTable [info.GetBatteryCount () % 8, hingefallsTable [info.GetOnIndicators().Count() % 8, randCount % 8] % 8] * 10) + hingefallsTable [info.GetOnIndicators().Count() % 8, hingefallsTable [info.GetOnIndicators().Count() % 8, randCount % 8] % 8];
					Debug.LogFormat ("[Module Name #{0}] Ind is middle", ModuleId);
				}
				else if(info.GetPortCount() % 8 + 1 <= info.GetBatteryCount() % 8 + 1 && info.GetPortCount() % 8 + 1 <= info.GetOnIndicators().Count() % 8 + 1)
				{
					output = (hingefallsTable [info.GetPortCount () % 8, hingefallsTable [info.GetPortCount () % 8, randCount % 8] % 8] * 100) + (hingefallsTable [info.GetBatteryCount () % 8, hingefallsTable [info.GetPortCount () % 8, randCount % 8] % 8] * 10) + hingefallsTable [info.GetOnIndicators().Count() % 8, hingefallsTable [info.GetPortCount (), randCount % 8] % 8];
					Debug.LogFormat ("[Module Name #{0}] Port is top (NO MIDDLE)", ModuleId);
				}
				else if(info.GetBatteryCount() % 8 + 1 <= info.GetPortCount() % 8 + 1 && info.GetBatteryCount() % 8 + 1 <= info.GetOnIndicators().Count() % 8 + 1)
				{
					output = (hingefallsTable [info.GetPortCount () % 8, hingefallsTable [info.GetBatteryCount () % 8, randCount % 8] % 8] * 100) + (hingefallsTable [info.GetBatteryCount () % 8, hingefallsTable [info.GetBatteryCount () % 8, randCount % 8] % 8] * 10) + hingefallsTable [info.GetOnIndicators().Count() % 8, hingefallsTable [info.GetBatteryCount () % 8, randCount % 8] % 8];
					Debug.LogFormat ("[Module Name #{0}] Battery is top (NO MIDDLE)", ModuleId);
				}
				else if(info.GetOnIndicators().Count() % 8 + 1 <= info.GetPortCount() % 8 + 1 && info.GetOnIndicators().Count() % 8 + 1 <= info.GetBatteryCount() % 8 + 1)
				{
					output = (hingefallsTable [info.GetPortCount () % 8, hingefallsTable [info.GetOnIndicators().Count() % 8, randCount % 8] % 8] * 100) + (hingefallsTable [info.GetBatteryCount () % 8, hingefallsTable [info.GetOnIndicators().Count() % 8, randCount % 8] % 8] * 10) + hingefallsTable [info.GetOnIndicators().Count() % 8, hingefallsTable [info.GetOnIndicators().Count() % 8, randCount % 8] % 8];
					Debug.LogFormat ("[Module Name #{0}] Ind is top (NO MIDDLE)", ModuleId);
				}
				Debug.LogFormat ("[Module Name #{0}] The output is {1}", ModuleId, output);
			}
			if(material == screenCol[3])
			{
				Debug.LogFormat ("[Module Name #{0}] Module chosen is Blank Slate and the section used is TAPCODE", ModuleId);
				for (int i = 0; i < 5; i++) 
				{
					for (int j = 0; j < 3; j++) 
					{
						for (int k = 0; k < info.GetSerialNumberLetters().ToArray().Length; k++) 
						{
							if (tapcodeTable [randCount % 7, i].ToCharArray () [j] == info.GetSerialNumberLetters ().ToArray () [k].ToString().ToLower().ToCharArray()[0]) 
							{
								output = (Encode (tapcodeTable [randCount % 7, i])[0] * 100000) + (Encode (tapcodeTable [randCount % 7, i])[1] * 10000) + (Encode (tapcodeTable [randCount % 7, i])[2] * 1000) + (Encode (tapcodeTable [randCount % 7, i])[3] * 100) + (Encode (tapcodeTable [randCount % 7, i])[4] * 10) + Encode (tapcodeTable [randCount % 7, i])[5];
								tapsFound = true;
								Debug.LogFormat ("[Module Name #{0}] The output is {1}", ModuleId, output);
								break;
							}
						}
						if (tapsFound) 
						{
							break;
						}
					}
					if (tapsFound) 
					{
						break;
					}
				}
				if (!tapsFound) 
				{
					output = (Encode (tapcodeTable [randCount % 7, 4])[0] * 100000) + (Encode (tapcodeTable [randCount % 7, 4])[1] * 10000) + (Encode (tapcodeTable [randCount % 7, 4])[2] * 1000) + (Encode (tapcodeTable [randCount % 7, 4])[3] * 100) + (Encode (tapcodeTable [randCount % 7, 4])[4] * 10) + Encode (tapcodeTable [randCount % 7, 4])[5];
					tapsFound = true;
					Debug.LogFormat ("[Module Name #{0}] The output is {1}", ModuleId, output);
				}
			}
		}

		if(htmlList[randHTML] == "<ol>1<li>1" || htmlList[randHTML] == "<ul>1<li>1" || htmlList[randHTML] == "<h3>1" || htmlList[randHTML] == "<tr>2<th>1")
		{
			switch (randCount % 7) 
			{
			case 0:
				posy = 2;
				break;
			case 1:
				posy = 2;
				posx = 2;
				break;
			case 2:
				posx = 2;
				break;
			case 3:
				posy = -2;
				posx = 2;
				break;
			case 4:
				posy = -2;
				break;
			case 5:
				posx = -2;
				break;
			case 6:
				posy = 2;
				posx = -2;
				break;
			}
			if (material == screenCol [0]) 
			{
				posx += 2;
				posy += 2;
			}
			if (material == screenCol [1]) 
			{
				posy += -2;
			}
			if (material == screenCol [2]) 
			{
				posx += 2;
			}
			if (material == screenCol [3]) 
			{
				posy += 2;
			}

			if (posx > 2) 
			{
				posx = posx - 5;
			}
			if (posy > 2) 
			{
				posy = posy - 5;
			}
			if (posx < -2) 
			{
				posx = posx + 5;
			}
			if (posy < -2) 
			{
				posy = posy + 5;
			}
			Debug.LogFormat ("[Module Name #{0}] Module chosen is Hertz and the movement in X axis is {1}, the movement in Y axis is {2}", ModuleId, posx, posy);

			modify1 = hertzTable1 [2 - posy, 2 + posx];
			modify2 = hertzTable2 [2 - posy, 2 + posx];
			modify2 = (modify2 + 1) * (1 + info.GetPortPlateCount ());
			output = (modify1 / modify2) % 64;
			output = (long)Math.Truncate (Convert.ToDouble(output));
			output = long.Parse (Convert.ToString (output, 2));
			Debug.LogFormat ("[Module Name #{0}] The output is {1}", ModuleId, output);
		}

		if(htmlList[randHTML] == "<ul>1<li>2" || htmlList[randHTML] == "<ul>1<li>3" || htmlList[randHTML] == "<tr>3" || htmlList[randHTML] == "<p>2" || htmlList[randHTML] == "<p>3" || htmlList[randHTML] == "<h3>5" || htmlList[randHTML] == "<tr>2<td>1" || htmlList[randHTML] == "<tr>4<td>1" || htmlList[randHTML] == "<tr>9")
		{
			stageNum = input.ToString ().ToCharArray ().Length / 2 + 1;
			curNum = 0;
			foreach (char letter in info.GetSerialNumberLetters().ToArray()) 
			{
				foreach (string ind in info.GetOnIndicators().ToArray()) 
				{
					for (int i = 0; i < 3; i++) 
					{
						if (ind.ToCharArray () [i] == letter) 
						{
							curNum += 2;
							indFound = true;
							break;
						}
					}
					if (indFound == true) 
					{
						indFound = false;
						break;
					}
				}
			}
			int increment = -1;
			foreach (string port in info.GetPorts().ToArray())
			{
				increment++;
				for (int i = increment; i < info.GetPortCount (); i++) 
				{
					if (i != increment && port == info.GetPorts ().ToArray() [i]) 
					{
						curNum = curNum - 4;
						portFound = true;
						break;
					}
				}
				if (portFound == true) 
				{
					portFound = false;
					break;
				}
			}
			curNum = curNum + (info.GetBatteryCount () * info.GetPortCount ());
			curNum = curNum * stageNum;
			while (curNum < 0) 
			{
				curNum = curNum + 25;
			}
			if (curNum < 10) 
			{
				curNum = curNum * 10;
			}
			if (curNum == 0) 
			{
				curNum = 10;
			}
			while (curNum > 49) 
			{
				curNum = curNum - 25;
			}

			string curstring = curNum.ToString ();
			colorbools [randCol] = true;
			colorbools [(randCol + 1) % 4] = true;
			bool isTableA = true;

			if(randCount % 8 + 1 == 3 && colorbools[info.GetBatteryHolderCount() % 4] ^ colorbools[info.GetPortPlateCount() % 4])
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 4 && !(colorbools[info.GetBatteryHolderCount() % 4] && colorbools[info.GetPortPlateCount() % 4]))
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 5 && !(colorbools[info.GetBatteryHolderCount() % 4] || colorbools[info.GetPortPlateCount() % 4]))
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 6 && colorbools[info.GetBatteryHolderCount() % 4] == colorbools[info.GetPortPlateCount() % 4])
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 7 && !(colorbools[info.GetBatteryHolderCount() % 4] && !colorbools[info.GetPortPlateCount() % 4]))
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 8 && !(!colorbools[info.GetBatteryHolderCount() % 4] && colorbools[info.GetPortPlateCount() % 4]))
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 1 && (colorbools[info.GetBatteryHolderCount() % 4] && colorbools[info.GetPortPlateCount() % 4] == true))
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else if(randCount % 8 + 1 == 2 && (colorbools[info.GetBatteryHolderCount() % 4] || colorbools[info.GetPortPlateCount() % 4] == true))
			{
				requiredCol = simontableA [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
			}
			else
			{
				requiredCol = simontableB [int.Parse (curstring.ToCharArray () [0].ToString ()), int.Parse (curstring.ToCharArray () [1].ToString ())];
				isTableA = false;
				Debug.LogFormat ("[Module Name #{0}] Module chosen is Simon's statement and the boolean evaluated false due to operation {1} " +
					"and the colours {2} and {3} being true, where {2} was the display colour", ModuleId, randCount % 8 + 1, randCol, (randCol + 1) % 4);
			}

			if(isTableA)
			{
                Debug.LogFormat("[Module Name #{0}] Module chosen is Simon's statement and the boolean evaluated true due to operation {1} " +
                    "and the colours {2} and {3} being true, where {2} was the display colour", ModuleId, randCount % 8 + 1, randCol, (randCol + 1) % 4);
            }

			if (requiredCol == 3) 
			{
				requiredCol = 2;
			}
			else if (requiredCol == 2) 
			{
				requiredCol = 0;
			}
			else if (requiredCol == 1) 
			{
				requiredCol = 3;
			}
			else if (requiredCol == 0) 
			{
				requiredCol = 1;
			}

			output = output * 100 + curNum;
			curNum = (int)output;
			Debug.LogFormat ("[Module Name #{0}] The output for now is {1}, and required colour being {2}", ModuleId, output, requiredCol + 1);
			simonException = true;
		}

		if (htmlList [randHTML] == "<ul>2<li>2<em>1" || htmlList [randHTML] == "<h3>2" || htmlList [randHTML] == "<p>4" || htmlList [randHTML] == "<p>6" || htmlList [randHTML] == "<ul>3<li>3") 
		{
			letters [0] = info.GetSerialNumberLetters ().ToArray () [0];
			if (material == screenCol [0]) {letters [1] = 'B'; letters [2] = 'L';}
			if (material == screenCol [1]) {letters [1] = 'R'; letters [2] = 'E';}
			if (material == screenCol [2]) {letters [1] = 'Y'; letters [2] = 'E';}
			if (material == screenCol [3]) {letters [1] = 'G'; letters [2] = 'R';}
			Debug.LogFormat ("[Module Name #{0}] The letters are {0}, {1} and {2}, with amount of statements checked being {3}", letters [0], letters [1], letters [2], randCount % 4 + 2);
			for (int i = 0; i < (randCount % 4 + 2); i++) 
			{
				if (i == 0 && (letters[0] == 'A' || letters[0] == 'E' || letters[0] == 'I' || letters[0] == 'O' || letters[0] == 'U')) 
				{
					upstatements++;
				}
				if (i == 1) 
				{
					int primetotal = 0;
					for(int j = 0; j < 3; j++)
					{
						int[] primecheck = Encode (letters [j].ToString ());
						if (primecheck [0] < 3 && letters [j] != 'K') 
						{
							primetotal = primetotal + ((primecheck [0] - 1) * 5) + primecheck [1];
						}
						if (letters [j] == 'K') 
						{
							primetotal += 11;
						}
						if (primecheck [0] > 2) 
						{
							primetotal = primetotal + ((primecheck [0] - 1) * 5) + primecheck [1] + 1;
						}
					}
					var limit = Math.Ceiling(Math.Sqrt(primetotal));
					bool compositefound = false;

					for (int j = 2; j <= limit; j++)  
					{
						if (primetotal % j == 0) 
						{
							compositefound = true;
						}
					}
					if (compositefound == false) 
					{
						upstatements++;
					}
				}
				if (i == 2 && (letters[0] != letters[1] && letters[0] != letters[2] && letters[1] != letters[2])) 
				{
					upstatements++;
				}
				if (i == 3) 
				{
					for (int j = 0; j < info.GetSerialNumberLetters ().ToArray ().Length; j++) 
					{
						if (letters [1] == info.GetSerialNumberLetters ().ToArray () [j]) 
						{
							upstatements++;
							break;
						}
					}
				}
				if (i == 4 && (letters[0] == 'U' || letters[1] == 'U' || letters[2] == 'U')) 
				{
					upstatements++;
				}
			}

			for (int i = 0; i < (randCount % 4 + 2); i++) 
			{
				if (i == 0)
				{
					bool vowelfound = false;
					for (int j = 0; j < 3; j++) 
					{
						if (letters [j] == 'A' || letters [j] == 'E' || letters [j] == 'I' || letters [j] == 'O' || letters [j] == 'U') 
						{
							vowelfound = true;
						}
					}
					if (vowelfound == false) 
					{
						leftstatements++;
					}
				}
				if (i == 1) 
				{
					int oddtotal = 0;
					for(int j = 0; j < 3; j++)
					{
						int[] oddcheck = Encode (letters [j].ToString ());
						if (oddcheck [0] < 3 && letters [j] != 'K') 
						{
							oddtotal = oddtotal + ((oddcheck [0] - 1) * 5) + oddcheck [1];
						}
						if (letters [j] == 'K') 
						{
							oddtotal += 11;
						}
						if (oddcheck [0] > 2) 
						{
							oddtotal = oddtotal + ((oddcheck [0] - 1) * 5) + oddcheck [1] + 1;
						}
					}
					if (oddtotal % 2 == 1) 
					{
						leftstatements++;
					}
				}
				if (i == 2 && letters[0] == letters[2]) 
				{
					leftstatements++;
				}
				if (i == 3)
				{
					for (int j = 0; j < info.GetSerialNumberLetters ().ToArray ().Length; j++) 
					{
						if (letters [2] == info.GetSerialNumberLetters ().ToArray () [j]) 
						{
							leftstatements++;
							break;
						}
					}
				}
				if (i == 4 && (letters[0] == 'L' || letters[1] == 'L' || letters[2] == 'L')) 
				{
					leftstatements++;
				}
			}

			for (int i = 0; i < (randCount % 4 + 2); i++) 
			{
				if (i == 0 && (letters[1] == 'A' || letters[1] == 'E' || letters[1] == 'I' || letters[1] == 'O' || letters[1] == 'U')) 
				{
					downstatements++;
				}
				if (i == 1) 
				{
					int total = 0;
					for(int j = 0; j < 3; j++)
					{
						int[] check = Encode (letters [j].ToString ());
						if (check [0] < 3 && letters [j] != 'K') 
						{
							total = total + ((check [0] - 1) * 5) + check [1];
						}
						if (letters [j] == 'K') 
						{
							total += 11;
						}
						if (check [0] > 2) 
						{
							total = total + ((check [0] - 1) * 5) + check [1] + 1;
						}
					}
					List<double> totaldigits = new List<double>();
					for (int j = 0; j < total.ToString ().Length; j++) 
					{
						totaldigits.Add(char.GetNumericValue(total.ToString ().ToCharArray () [j]));
					}
					if (totaldigits.Count == 1) 
					{
						print (total);
						print (totaldigits [0]);
						if (total % (int)totaldigits [0] == 0) 
						{
							downstatements++;
						}
					}
					else
					{
						print (total);
						print (totaldigits [0]);
						print (totaldigits [1]);
						if ((totaldigits [0] + totaldigits [1]).ToString ().Length != 2)
						{
							if (total % ((int)totaldigits [0] + (int)totaldigits [1]) == 0) 
							{
								downstatements++;
							}
						}
						else
						{
							if (total % ((int)char.GetNumericValue((totaldigits [0] + totaldigits [1]).ToString().ToCharArray()[0]) + (int)char.GetNumericValue((totaldigits [0] + totaldigits [1]).ToString().ToCharArray()[1])) == 0) 
							{
								downstatements++;
							}
						}
					}
				}
				if (i == 2 && letters[0] == letters[1]) 
				{
					downstatements++;
				}
				if (i == 3) 
				{
					bool serialfound = false;
					for (int j = 0; j < info.GetSerialNumberLetters ().ToArray ().Length; j++) 
					{
						if (letters [0] == info.GetSerialNumberLetters ().ToArray () [j] || letters [1] == info.GetSerialNumberLetters ().ToArray () [j] || letters [2] == info.GetSerialNumberLetters ().ToArray () [j]) 
						{
							serialfound = true;
						}
					}
					if (serialfound == false) 
					{
						downstatements++;
					}
				}
				if (i == 4 && (letters[0] == 'D' || letters[1] == 'D' || letters[2] == 'D')) 
				{
					downstatements++;
				}
			}

			for (int i = 0; i < (randCount % 4 + 2); i++) 
			{
				if (i == 0 && (letters[2] == 'A' || letters[2] == 'E' || letters[2] == 'I' || letters[2] == 'O' || letters[2] == 'U')) 
				{
					rightstatements++;
				}
				if (i == 1) 
				{
					int total = 0;
					for(int j = 0; j < 3; j++)
					{
						int[] check = Encode (letters [j].ToString ());
						if (check [0] < 3 && letters [j] != 'K') 
						{
							total = total + ((check [0] - 1) * 5) + check [1];
						}
						if (letters [j] == 'K') 
						{
							total += 11;
						}
						if (check [0] > 2) 
						{
							total = total + ((check [0] - 1) * 5) + check [1] + 1;
						}
					}
					if (total % 7 == 0) 
					{
						rightstatements++;
					}
				}
				if (i == 2 && letters[1] == letters[2]) 
				{
					rightstatements++;
				}
				if (i == 3) 
				{
					for (int j = 0; j < info.GetSerialNumberLetters ().ToArray ().Length; j++) 
					{
						if (letters [0] == info.GetSerialNumberLetters ().ToArray () [j]) 
						{
							rightstatements++;
							break;
						}
					}
				}
				if (i == 4 && (letters[0] == 'R' || letters[1] == 'R' || letters[2] == 'R')) 
				{
					rightstatements++;
				}
			}

			arraystatements [0] = upstatements;
			arraystatements [1] = rightstatements;
			arraystatements [2] = downstatements;
			arraystatements [3] = leftstatements;
			Array.Sort (arraystatements);
			output = (arraystatements [3] * 1000) + (arraystatements [0] * 100) + (arraystatements [1] * 10) + arraystatements [2];
			Debug.LogFormat ("[Module Name #{0}] Module chosen is Not green arrows and statement amounts (in the order up,right,down and left) are {1}, {2}, {3} and {4}", ModuleId, upstatements, rightstatements, downstatements, leftstatements);
			Debug.LogFormat ("[Module Name #{0}] Output is {1}", ModuleId, output);
		}
	}

	void octaToGrey(int repeat)
	{
		for(int i = 1; i < repeat; i++)
		{
			if (outputchar [i - 1] == '1' && outputchar[i] == '1') 
			{
				outputchar [i] = '0';
			}
			else if (outputchar [i - 1] == '1' && outputchar[i] == '0') 
			{
				outputchar [i] = '1';
			}
		}
		string outputstring = new string (outputchar);
		output = long.Parse (outputstring);
		Debug.LogFormat("[Module Name #{0}] Gray code is {1}", ModuleId, output);
	}
	void selectPress(KMSelectable pressedButton)
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		pressedButton.AddInteractionPunch (0.5f);
		int buttonPosition = Array.IndexOf(selectables, pressedButton);

		if(moduleSelect[0].gameObject.activeSelf == false && _isSolved == false)
		{
			switch (buttonPosition) 
			{
			case 0:
				if (input != 0 && input < 100000000) 
				{
					input = input * 10;
				}
				else
				{
					input = 0;
				}
				break;
			case 1:
				if (input < 100000000) 
				{
					input = input * 10 + 1;
				}
				else
				{
					input = 1;
				}
				break;
			case 2:
				if (input < 100000000) 
				{
					input = input * 10 + 2;
				}
				else
				{
					input = 2;
				}
				break;
			case 3:
				if (input < 100000000) 
				{
					input = input * 10 + 3;
				}
				else
				{
					input = 3;
				}
				break;
			case 4:
				if (input < 100000000) 
				{
					input = input * 10 + 4;
				}
				else
				{
					input = 4;
				}
				break;
			case 5:
				if (input < 100000000) 
				{
					input = input * 10 + 5;
				}
				else
				{
					input = 5;
				}
				break;
			case 6:
				if (input < 100000000) 
				{
					input = input * 10 + 6;
				}
				else
				{
					input = 6;
				}
				break;
			case 7:
				if (input < 100000000) 
				{
					input = input * 10 + 7;
				}
				else
				{
					input = 7;
				}
				break;
			case 8:
				if (input < 100000000) 
				{
					input = input * 10 + 8;
				}
				else
				{
					input = 8;
				}
				break;
			case 9:
				if (input < 100000000) 
				{
					input = input * 10 + 9;
				}
				else
				{
					input = 9;
				}
				break;
			case 10:
				if (blankslateException == true) 
				{
					if (input.ToString ().ToCharArray ().Length < 5) 
					{
						incorrect = true;
						Reset ();
						break;
					}
					for (int i = 0; i < 5; i++)
					{
						if (input.ToString ().ToCharArray () [i] == ((randCount + info.GetPortPlateCount ()) % 10).ToString ().ToCharArray () [0] || input.ToString ().ToCharArray () [i] == ((randCount + info.GetBatteryCount ()) % 10).ToString ().ToCharArray () [0] || input.ToString ().ToCharArray () [i] == ((randCount + info.GetSolvedModuleNames ().Count) % 10).ToString ().ToCharArray () [0] || input.ToString ().ToCharArray () [i] == ((randCount + info.GetBatteryHolderCount ()) % 10).ToString ().ToCharArray () [0]) 
						{
							incorrect = true;
							Reset ();
							break;
						}
					}
					if (input == 0) 
					{
						break;
					}
					bool[] seen = new bool[10];
					for (int i = 0; i < 5; i++)
					{
						for (int j = 0; j < 10; j++) 
						{
							if (input.ToString ().ToCharArray () [i] == j.ToString().ToCharArray()[0]) 
							{
								if (seen [j] == true)
								{
									incorrect = true;
									blankslateException = false;
									break;
								}
								else 
								{
									seen [j] = true;
								}
							}
						}
						if (incorrect == true)
						{
							break;
						}
					}
					if (incorrect == true)
					{
						Reset ();
						break;
					}
					for (int i = 0; i < 10; i++) 
					{
						seen [i] = false;
					}

					stage++;
					ding.Play ();
					if (stage == 2) 
					{
						module.HandlePass ();
						Log ("Solved!");
						_isSolved = true;
					}
				}
				else if(simonException == true && input == output)
				{
					if (screenCol [randCol] == screenCol[requiredCol] || input.ToString().ToCharArray().Length == 8) 
					{
						stage++;
						ding.Play ();
						if (stage == 2) 
						{
							module.HandlePass ();
							Log ("Solved!");
							_isSolved = true;
						}
					}
					else
					{
						incorrect = true;
					}
				}
				if (input == output && blankslateException == false && simonException == false) 
				{
					stage++;
					ding.Play ();
					if (stage == 2) 
					{
						module.HandlePass ();
						Log ("Solved!");
						_isSolved = true;
					}
				} 
				else if (input != output && blankslateException == false) 
				{
					incorrect = true;
				}
				Reset ();
				break;
			}
			if (incorrect) 
			{
				module.HandleStrike ();
				Log ("Striked!");
				stage = 0;
				incorrect = false;
			}
			Displays [1].text = input.ToString ();
		}
	}

	void Reset()
	{
		octaList1 = new int[] { 2, 6, 9 };
		octaList2 = new int[] { 2, 6, 9, 8, 2 };
		octaList3 = new int[] { 2, 6, 9, 8, 9, 9, 1 };
		octaList4 = new int[] { 2, 6, 9, 8, 9, 9, 5, 3, 9 };
		blankslateException = false;
		tapsFound = false;
		posx = 0;
		posy = 0;
		modify1 = 0;
		modify2 = 0;
		curNum = 0;
		stageNum = 0;
		simonException = false;
		indFound = false;
		portFound = false;
		requiredCol = 0;
		upstatements = 0;
		rightstatements = 0;
		downstatements = 0;
		leftstatements = 0;
		input = 0;
		output = 0;
		Displays [1].text = "0";
		Displays [1].gameObject.SetActive (false);
		moduleSelect [0].gameObject.SetActive (true);
		Screen.transform.localPosition = new Vector3 (Screen.transform.localPosition.x, Screen.transform.localPosition.y - 0.022f, Screen.transform.localPosition.z);
		foreach (KMSelectable button in selectables) {
			button.gameObject.transform.localPosition = new Vector3 (button.gameObject.transform.localPosition.x, button.gameObject.transform.localPosition.y - 0.02f, button.gameObject.transform.localPosition.z);
		}
		pressCount = 0;
		randCount = Rnd.Range (2, 12);
		randHTML = Rnd.Range (0, 32);
		randCol = Rnd.Range (0, 4);
	}

	int[] Encode(string s)
	{
		var mc = new List<int>();
		foreach (char c in s.ToUpperInvariant())
		{
			switch (c) 
			{
			case 'A':
				mc.Add (1);
				mc.Add (1);
				break;
			case 'B':
				mc.Add (1);
				mc.Add (2);
				break;
			case 'C':
				mc.Add (1);
				mc.Add (3);
				break;
			case 'D':
				mc.Add (1);
				mc.Add (4);
				break;
			case 'E':
				mc.Add(1);
				mc.Add(5);
				break;
			case 'F':
				mc.Add(2);
				mc.Add(1);
				break;
			case 'G':
				mc.Add(2);
				mc.Add(2);
				break;
			case 'H':
				mc.Add(2);
				mc.Add(3);
				break;
			case 'I':
				mc.Add(2);
				mc.Add(4);
				break;
			case 'J':
				mc.Add(2);
				mc.Add(5);
				break;
			case 'K':
				mc.Add(1);
				mc.Add(3);
				break;
			case 'L':
				mc.Add(3);
				mc.Add(1);
				break;
			case 'M':
				mc.Add(3);
				mc.Add(2);
				break;
			case 'N':
				mc.Add(3);
				mc.Add(3);
				break;
			case 'O':
				mc.Add(3);
				mc.Add(4);
				break;
			case 'P':
				mc.Add(3);
				mc.Add(5);
				break;
			case 'Q':
				mc.Add (4);
				mc.Add (1);
				break;
			case 'R':
				mc.Add(4);
				mc.Add(2);
				break;
			case 'S':
				mc.Add(4);
				mc.Add(3);
				break;
			case 'T':
				mc.Add(4);
				mc.Add (4);
				break;
			case 'U':
				mc.Add(4);
				mc.Add(5);
				break;
			case 'V':
				mc.Add(5);
				mc.Add(1);
				break;
			case 'W':
				mc.Add(5);
				mc.Add(2);
				break;
			case 'X':
				mc.Add(5);
				mc.Add(3);
				break;
			case 'Y':
				mc.Add(5);
				mc.Add(4);
				break;
			case 'Z':
				mc.Add(5);
				mc.Add(5);
				break;
			}
		}
		return mc.ToArray();
	}

	void Log(string message)
	{
		Debug.LogFormat("[Module Name #{0}] {1}", ModuleId, message);
	}

	#pragma warning disable 414
	private string TwitchHelpMessage = "!{0} colo(u)rblind [toggles colourblind support], !{0} press [Presses the module if active], !{0} pressbutton # [Presses the specified button if active (10 is display)].";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		int num;
		string[] parameters = command.Split(' ');
		parameters [0] = parameters [0].ToLower ();

		if (parameters [0] == "colourblind" || parameters [0] == "colorblind") 
		{
			if (parameters.Length > 1)
			{
				yield return "sendtochaterror Too many parameters!";
				yield break;
			}
			else 
			{
				yield return null;
				colourblindSupportOn = !colourblindSupportOn;
				yield break;
			}
		}
		else if (parameters [0] == "press") 
		{
			if (parameters.Length > 1)
			{
				yield return "sendtochaterror Too many parameters!";
				yield break;
			}
			else if(!moduleSelect[0].gameObject.activeInHierarchy)
			{
				yield return "sendtochaterror Module press not active!";
				yield break;
			}
			else 
			{
				yield return null;
				moduleSelect [0].OnInteract ();
				yield break;
			}
		}
		else if (parameters [0] == "pressbutton") 
		{
			if (parameters.Length < 2)
			{
				yield return "sendtochaterror Not enough parameters!";
				yield break;
			}
			if (parameters.Length > 2)
			{
				yield return "sendtochaterror Too many parameters!";
				yield break;
			}
			else if(pressCount != randCount + 1)
			{
				yield return "sendtochaterror Buttons not active!";
				yield break;
			}
			else if(int.TryParse(parameters[1], out num) == false)
			{
				yield return "sendtochaterror Second parameter is not a number!";
				yield break;
			}
			else if(int.Parse(parameters[1]) < 0 || int.Parse(parameters[1]) > 10)
			{
				yield return "sendtochaterror Second parameter is not in range!";
				yield break;
			}
			else
			{
				yield return null;
				selectables [int.Parse (parameters [1])].OnInteract ();
				yield break;
			}
		}
		else
		{
			yield return "sendtochaterror That command does not exist!";
			yield break;
		}
	}
}
