using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class Stack : MonoBehaviour {
	//方块长宽
	private const float BOUNDS_SIZE = 3.5f;
	//方块组下移速度
	private const float STACK_MOVING_SPEED = 5.0f;
	//方块对位的偏移量，超过则切割
	private const float ERROR_MARGIN =  0.3f;
	private const float STACK_BOUND_GAIN =  0.25f;
	//连续COMBO_START_GAIN次combo时扩大方块特定方向的面积
	private const int COMBO_START_GAIN =  3;

	public AudioSource audioSource;
	//游戏结束面板
	public GameObject gameOverObject;
	//游戏分数
	public Text scoreText;
	//方块材质
	public Material stackMaterial;
	//方块组
	private GameObject[] stack;
	private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

	//方块组索引
	private int stackIndex;
	private int scoreCount = 0;
	//当前COMBO数
	private int combo =  0;

	private float tileTransition = 0.0f;
	//方块移动速度
	private float tileSpeed = 2.5f;
	//标记放下方块的位置
	private float secondaryPosition;

	//是否在X轴移动
	private bool isMovingOnX = true;
	private bool isGameOver = false;

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;

	public Color32[] stackColors = new Color32[4];

	// Use this for initialization
	void Start () {
		stack = new GameObject[transform.childCount];
		for(int i = 0; i < transform.childCount; i++) {
			stack [i] = transform.GetChild (i).gameObject;
			//初始化方块颜色
			ColorMess(stack [i].GetComponent<MeshFilter> ().mesh);
		}
		stackIndex = transform.childCount - 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) {
			if (PlaceTile()) {
				SpawnTile ();
				scoreCount++;
				scoreText.text = scoreCount.ToString ();
			} else {
				GameOver ();
			}
		}

		MoveTile ();

		//镜头跟随，使用插值，让跟随动作圆滑或者缓冲的效果。用公式表示就是：from + （to - from） * t
		transform.position = Vector3.Lerp (transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);
	}

	//生成碎片方块
	private void CreateRubble(Vector3 position, Vector3 scale) {
		GameObject rubberObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
		rubberObject.transform.localPosition = position;
		rubberObject.transform.localScale = scale;
		rubberObject.AddComponent<Rigidbody> ();
		rubberObject.GetComponent<MeshRenderer> ().material = stackMaterial;
		ColorMess (rubberObject.GetComponent<MeshFilter> ().mesh);
	}
		
	//移动方块
	private void MoveTile() {
		if (isGameOver) {
			return;
		}

		tileTransition += Time.deltaTime * tileSpeed;
		if (isMovingOnX) {
			//最上面方块沿X轴做sin函数轨迹值移动
			stack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin (tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
		} else {
			stack [stackIndex].transform.localPosition = new Vector3 (secondaryPosition, scoreCount, Mathf.Sin (tileTransition) * BOUNDS_SIZE);
		}
	}

	//放置方块
	private bool PlaceTile() {
		Transform t = stack[stackIndex].transform;
		if (isMovingOnX) {
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs (deltaX) > ERROR_MARGIN) {
				//切割方块
				combo = 0;
				stackBounds.x -= Mathf.Abs (deltaX);
				if (stackBounds.x <= 0) {
					return false;
				}

				float middle = (lastTilePosition.x + t.localPosition.x) / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
				CreateRubble (
					new Vector3( t.position.x > 0
						? t.position.x + t.localScale.x/2
						: t.position.x - t.localScale.x/2, t.position.y, t.position.z),
					new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
				);
				t.localPosition = new Vector3 (middle, scoreCount, lastTilePosition.z);
			} else {
				//连续COMBO_START_GAIN次combo时扩大方块x方向的面积
				if (combo > COMBO_START_GAIN) {
					stackBounds.x += STACK_BOUND_GAIN;
					if (stackBounds.x > BOUNDS_SIZE) {
						stackBounds.x = BOUNDS_SIZE;
					}
					float middle = (lastTilePosition.x + t.localPosition.x) / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (middle, scoreCount, lastTilePosition.z);
				}
				combo++;
				t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		} else {
			float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs(deltaZ) > ERROR_MARGIN) {
				//切割方块
				combo = 0;
				stackBounds.y -= Mathf.Abs (deltaZ);
				if (stackBounds.y <= 0) {
					return false;
				}

				float middle = (lastTilePosition.z + t.localPosition.z) / 2;
				t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
				CreateRubble (
					new Vector3( t.position.x, t.position.y, 
						t.position.z > 0
						? t.position.z + t.localScale.z/2
						: t.position.z - t.localScale.z/2),
					new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
				);
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle);
			} else {
				//连续COMBO_START_GAIN次combo时扩大方块z方向的面积
				if (combo > COMBO_START_GAIN) {
					stackBounds.y += STACK_BOUND_GAIN;
					if (stackBounds.y > BOUNDS_SIZE) {
						stackBounds.y = BOUNDS_SIZE;
					}
					float middle = (lastTilePosition.z + t.localPosition.z) / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle);
				}
				combo++;
				t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		}

		//标记放下方块时的移动位置，以便能在当前位置改变移动方向
		secondaryPosition = isMovingOnX ? t.localPosition.x : t.localPosition.z;
		//改变移动方向
		isMovingOnX = !isMovingOnX;
		return true;
	}

	private void SpawnTile() {
		if (null != audioSource) {
			audioSource.Play ();
		}
		lastTilePosition = stack [stackIndex].transform.localPosition;
		stackIndex--;
		if (stackIndex < 0) {
			stackIndex = transform.childCount - 1;
		}
		//整个Stack下移位置
		desiredPosition = Vector3.down * scoreCount;
		//将最下面的方块移动到最上面
		stack [stackIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
		//生成的方块跟最上面那一块大小一样
		stack [stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

		ColorMess (stack [stackIndex].GetComponent<MeshFilter> ().mesh);
	}

	private void ColorMess(Mesh mesh) {
		Vector3[] vecties = mesh.vertices;
		Color32[] colors = new Color32[vecties.Length];

		float f = Mathf.Sin (scoreCount * 0.25f);
		for (int i = 0; i < vecties.Length; i++) {
			colors [i] = Lerp4 (stackColors[0], stackColors[1], stackColors[2], stackColors[3], f);
		}

		//网格的顶点颜色
		mesh.colors32 = colors;
	}

	/**
	 * Color.Lerp插值:通过t在颜色a和b之间线性插值。"t"是在0到1之间的值，当t是0时返回颜色a。当t是1时返回颜色b。
	 */
	private Color32 Lerp4 (Color32 a, Color32 b, Color32 c, Color32 d, float t) {
		if (t < 0.33) {
			return Color.Lerp (a, b, t / 0.33f);
		} else if (t < 0.66f) {
			return Color.Lerp (b, c, (t - 0.33f) / 0.33f);
		} else {
			return Color.Lerp (c, d, (t - 0.66f) / 0.66f);
		}
	}

	private void GameOver() {
		Debug.Log ("Game Over");
		if (PlayerPrefs.GetInt("score") < scoreCount) {
			PlayerPrefs.SetInt ("score", scoreCount);
		}
		isGameOver = true;
		gameOverObject.SetActive (true);
		stack [stackIndex].AddComponent <Rigidbody> ();
	}

	public void OnButtonClick(string sceneName) {
		SceneManager.LoadScene (sceneName);
	}
}
