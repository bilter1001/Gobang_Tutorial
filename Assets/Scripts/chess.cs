///**********************************************************************
///作者信息
///开发者：雁回晴空
///时间：2017.01.11
///联系方式：http://blog.csdn.net/zzlyw/article/details/54345250
///**********************************************************************

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class chess : MonoBehaviour {

	//四个锚点位置，用于计算棋子落点
    public GameObject LeftTop;
    public GameObject RightTop;
    public GameObject LeftBottom;
    public GameObject RightBottom;
    //主摄像机
    public Camera cam;
    //锚点在屏幕上的映射位置
    Vector3 LTPos;
    Vector3 RTPos;
    Vector3 LBPos;
    Vector3 RBPos;

    Vector3 PointPos;//当前点选的位置
    float gridWidth =1; //棋盘网格宽度
    float gridHeight=1; //棋盘网格高度
    float minGridDis;  //网格宽和高中较小的一个
    Vector2[,] chessPos; //存储棋盘上所有可以落子的位置
    int[,] chessState; //存储棋盘位置上的落子状态
    Vector2 curCheesPos;//当前落子位置
    enum turn {black, white } ;
    turn chessTurn; //落子顺序
    public Texture2D white; //白棋子
    public Texture2D black; //黑棋子
    public Texture2D ring; //圆环
    public Texture2D blackWin; //白子获胜提示图
    public Texture2D whiteWin; //黑子获胜提示图
    int winner = 0; //获胜方，1为黑子，-1为白子
    bool isPlaying = true; //是否处于对弈状态
    Stack<Vector2> stackSteps = new Stack<Vector2>();
    GUIStyle gUIStyle;
    void Start () {
        chessPos = new Vector2[15, 15];
        chessState =new int[15,15];
        chessTurn = turn.black;
        curCheesPos = new Vector2(-1,-1);;
    }
	
	void Update () {

        //计算锚点位置
        LTPos = cam.WorldToScreenPoint(LeftTop.transform.position);
        RTPos = cam.WorldToScreenPoint(RightTop.transform.position);
        LBPos = cam.WorldToScreenPoint(LeftBottom.transform.position);
        RBPos = cam.WorldToScreenPoint(RightBottom.transform.position);
        //计算网格宽度
        gridWidth = (RTPos.x - LTPos.x) / 14;
        gridHeight = (LTPos.y - LBPos.y) / 14;
        minGridDis = gridWidth < gridHeight ? gridWidth : gridHeight;
        //计算落子点位置
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                chessPos[i, j] = new Vector2(LBPos.x + gridWidth * i, LBPos.y + gridHeight * j);
            }
        }
        //检测鼠标输入并确定落子状态
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            PointPos = Input.mousePosition;
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {   
                    //找到最接近鼠标点击位置的落子点，如果空则落子
                    if (Dis(PointPos, chessPos[i, j]) < minGridDis / 2 && chessState[i,j]==0)
                    {
                        //根据下棋顺序确定落子颜色
                        chessState[i, j] = chessTurn == turn.black ? 1 : -1;
                        curCheesPos = new Vector2(i, j);
                        //将每一步落子，放入到栈里面
                        stackSteps.Push(curCheesPos);
                        //落子成功，更换下棋顺序
                        chessTurn = chessTurn == turn.black ? turn.white : turn.black;                        
                    }
                }
            }
            //调用判断函数，确定是否有获胜方
            int re = result();
            if (re == 1)
            {
                Debug.Log("黑棋胜");
                winner = 1;
                isPlaying = false;
            }
            else if(re==-1)
            {
                Debug.Log("白棋胜");
                winner = -1;
                isPlaying = false;
            }            
        }
        //按下空格重新开始游戏
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReStart();
        }     
	}

    //重新开始游戏
    void ReStart()
    {
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                chessState[i, j] = 0;
            }
        }
        isPlaying = true;
        chessTurn = turn.black;
        winner = 0;
        curCheesPos = new Vector2(-1, -1);
        stackSteps.Clear();
    }

    //悔棋
    void Undo()
    {
        //判断当前栈中是否存在以走步数,只有存在已走步数才能悔棋
        if (stackSteps.Count > 0)
        {
            //弹出最后一步,并将其位置重置为没有棋子的状态
            var lastStep = stackSteps.Pop();
            int x = (int)lastStep.x;
            int y = (int)lastStep.y;
            chessState[x,y] = 0;
            if (chessTurn == turn.black)
            {
                chessTurn = turn.white;
            }
            else
            {
                chessTurn = turn.black;
            }

            //并重新设置上一步为当前的棋子
            if (stackSteps.Count == 0)
            {
                curCheesPos.x = -1;
                curCheesPos.y = -1;
            }
            else
            {
                var prevStep = stackSteps.Peek();
                curCheesPos.x = prevStep.x;
                curCheesPos.y = prevStep.y;
            }

        }

    }

    //计算平面距离函数
    float Dis(Vector3 mPos, Vector2 gridPos)
    {
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2)+ Mathf.Pow(mPos.y - gridPos.y, 2));
    }

    void OnGUI()
    {

        GUIStyle buttonMid = GUI.skin.button;
        gUIStyle = buttonMid;
        gUIStyle.alignment = TextAnchor.MiddleCenter; //我们想让它居中对齐
        gUIStyle.fontSize = 20;
        //绘制当前落子的底图圆环
        if (curCheesPos.x != -1 || curCheesPos.y != -1)
        {
            int curX = (int)curCheesPos.x;
            int curY = (int)curCheesPos.y;
            float heigt = gridHeight + gridHeight / 6;
            float width = gridWidth + gridWidth / 6;
            GUI.DrawTexture(new Rect(chessPos[curX, curY].x - width / 2, Screen.height - chessPos[curX, curY].y - heigt / 2, width, heigt), ring,ScaleMode.ScaleToFit, true, 0, Color.red, 0, 0);
        }

        //绘制棋子
        for (int i=0;i<15;i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (chessState[i, j] == 1)
                {
                    GUI.DrawTexture(new Rect(chessPos[i,j].x-gridWidth/2, Screen.height-chessPos[i,j].y-gridHeight/2, gridWidth,gridHeight),black);
                }
                if (chessState[i, j] == -1)
                {
                    GUI.DrawTexture(new Rect(chessPos[i, j].x - gridWidth / 2, Screen.height - chessPos[i, j].y - gridHeight / 2, gridWidth, gridHeight), white);
                }               
            }
        }

        //根据获胜状态，弹出相应的胜利图片
        if (winner ==  1)
        GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), blackWin);
        if (winner == -1)
        GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), whiteWin);
        if (winner != 0)
        {

            if (GUI.Button(new Rect(Screen.width * 0.25f, Screen.height * 0.5f, Screen.width * 0.5f, Screen.height * 0.25f), "重开开始", gUIStyle))
            {
                ReStart();
            }            
        }

        if (GUI.Button(new Rect(Screen.width * 0.03f, Screen.height * 0.04f, Screen.width * 0.08f, Screen.height * 0.08f), "悔棋", gUIStyle))
        {
            Undo();
        }

        if (GUI.Button(new Rect(Screen.width * 0.03f, Screen.height * 0.86f, Screen.width * 0.09f, Screen.height * 0.08f), "重新开始", gUIStyle))
        {
            ReStart();
        }
    }

    //检测是够获胜的函数，不含黑棋禁手检测
    int result()
    {
        int flag = 0;
        //如果当前该白棋落子，标定黑棋刚刚下完一步，此时应该判断黑棋是否获胜
        if(chessTurn == turn.white)
        {
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (j < 4)
                    {
                        //横向
                        if (chessState[i, j] == 1 && chessState[i, j + 1] == 1 && chessState[i, j + 2] == 1 && chessState[i, j + 3] == 1 && chessState[i, j + 4] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //纵向
                        if (chessState[i, j] == 1 && chessState[i + 1, j] == 1 && chessState[i + 2, j] == 1 && chessState[i + 3, j] == 1 && chessState[i + 4, j] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //右斜线
                        if (chessState[i, j] == 1 && chessState[i + 1, j + 1] == 1 && chessState[i + 2, j + 2] == 1 && chessState[i + 3, j + 3] == 1 && chessState[i + 4, j + 4] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //左斜线
                        //if (chessState[i, j] == 1 && chessState[i + 1, j - 1] == 1 && chessState[i + 2, j - 2] == 1 && chessState[i + 3, j - 3] == 1 && chessState[i + 4, j - 4] == 1)
                        //{
                        //    flag = 1;
                        //    return flag;
                        //}
                    }
                    else if (j >= 4 && j < 11)
                    {
                        //横向
                        if (chessState[i, j] == 1 && chessState[i, j + 1] == 1 && chessState[i, j + 2] == 1 && chessState[i, j + 3] == 1 && chessState[i, j + 4] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //纵向
                        if (chessState[i, j] == 1 && chessState[i + 1, j] == 1 && chessState[i + 2, j] == 1 && chessState[i + 3, j] == 1 && chessState[i + 4, j] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //右斜线
                        if (chessState[i, j] == 1 && chessState[i + 1, j + 1] == 1 && chessState[i + 2, j + 2] == 1 && chessState[i + 3, j + 3] == 1 && chessState[i + 4, j + 4] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //左斜线
                        if (chessState[i, j] == 1 && chessState[i + 1, j - 1] == 1 && chessState[i + 2, j - 2] == 1 && chessState[i + 3, j - 3] == 1 && chessState[i + 4, j - 4] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                    }
                    else
                    {
                        //横向
                        //if (chessState[i, j] == 1 && chessState[i, j + 1] == 1 && chessState[i, j + 2] == 1 && chessState[i, j + 3] == 1 && chessState[i, j + 4] == 1)
                        //{
                        //    flag = 1;
                        //    return flag;
                        //}
                        //纵向
                        if (chessState[i, j] == 1 && chessState[i + 1, j] == 1 && chessState[i + 2, j] == 1 && chessState[i + 3, j] == 1 && chessState[i + 4, j] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                        //右斜线
                        //if (chessState[i, j] == 1 && chessState[i + 1, j + 1] == 1 && chessState[i + 2, j + 2] == 1 && chessState[i + 3, j + 3] == 1 && chessState[i + 4, j + 4] == 1)
                        //{
                        //    flag = 1;
                        //    return flag;
                        //}
                        //左斜线
                        if (chessState[i, j] == 1 && chessState[i + 1, j - 1] == 1 && chessState[i + 2, j - 2] == 1 && chessState[i + 3, j - 3] == 1 && chessState[i + 4, j - 4] == 1)
                        {
                            flag = 1;
                            return flag;
                        }
                    }

                }
            }
            for (int i = 11; i < 15; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    //只需要判断横向    
                    if (chessState[i, j] == 1 && chessState[i, j + 1] == 1 && chessState[i, j + 2] == 1 && chessState[i, j + 3] == 1 && chessState[i, j + 4] == 1)
                    {
                        flag = 1;
                        return flag;
                    }
                }
            }  
        }
        //如果当前该黑棋落子，标定白棋刚刚下完一步，此时应该判断白棋是否获胜
        else if(chessTurn == turn.black)
        {
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (j < 4)
                    {
                        //横向
                        if (chessState[i, j] == -1 && chessState[i, j + 1] == -1 && chessState[i, j + 2] == -1 && chessState[i, j + 3] == -1 && chessState[i, j + 4] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //纵向
                        if (chessState[i, j] == -1 && chessState[i + 1, j] == -1 && chessState[i + 2, j] == -1 && chessState[i + 3, j] == -1 && chessState[i + 4, j] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //右斜线
                        if (chessState[i, j] == -1 && chessState[i + 1, j + 1] == -1 && chessState[i + 2, j + 2] == -1 && chessState[i + 3, j + 3] == -1 && chessState[i + 4, j + 4] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //左斜线
                        //if (chessState[i, j] == -1 && chessState[i + 1, j - 1] == -1 && chessState[i + 2, j - 2] == -1 && chessState[i + 3, j - 3] == -1 && chessState[i + 4, j - 4] == -1)
                        //{
                        //    flag = -1;
                        //    return flag;
                        //}
                    }
                    else if (j >= 4 && j < 11)
                    {
                        //横向
                        if (chessState[i, j] == -1 && chessState[i, j + 1] == -1 && chessState[i, j + 2] == -1 && chessState[i, j + 3] == -1 && chessState[i, j + 4] ==- 1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //纵向
                        if (chessState[i, j] == -1 && chessState[i + 1, j] == -1 && chessState[i + 2, j] == -1 && chessState[i + 3, j] == -1 && chessState[i + 4, j] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //右斜线
                        if (chessState[i, j] == -1 && chessState[i + 1, j + 1] == -1 && chessState[i + 2, j + 2] == -1 && chessState[i + 3, j + 3] == -1 && chessState[i + 4, j + 4] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //左斜线
                        if (chessState[i, j] == -1 && chessState[i + 1, j - 1] == -1 && chessState[i + 2, j - 2] == -1 && chessState[i + 3, j - 3] == -1 && chessState[i + 4, j - 4] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                    }
                    else
                    {
                        //横向
                        //if (chessState[i, j] == -1 && chessState[i, j + 1] ==- 1 && chessState[i, j + 2] == -1 && chessState[i, j + 3] == -1 && chessState[i, j + 4] == -1)
                        //{
                        //    flag = -1;
                        //    return flag;
                        //}
                        //纵向
                        if (chessState[i, j] == -1 && chessState[i + 1, j] ==- 1 && chessState[i + 2, j] ==- 1 && chessState[i + 3, j] ==- 1 && chessState[i + 4, j] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                        //右斜线
                        //if (chessState[i, j] == -1 && chessState[i + 1, j + 1] == -1 && chessState[i + 2, j + 2] == -1 && chessState[i + 3, j + 3] == -1 && chessState[i + 4, j + 4] == -1)
                        //{
                        //    flag = -1;
                        //    return flag;
                        //}
                        //左斜线
                        if (chessState[i, j] == -1 && chessState[i + 1, j - 1] == -1 && chessState[i + 2, j - 2] == -1 && chessState[i + 3, j - 3] == -1 && chessState[i + 4, j - 4] == -1)
                        {
                            flag = -1;
                            return flag;
                        }
                    }
                }
            }
            for (int i = 11; i < 15; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    //只需要判断横向    
                    if (chessState[i, j] == -1 && chessState[i, j + 1] == -1 && chessState[i, j + 2] == -1 && chessState[i, j + 3] == -1 && chessState[i, j + 4] == -1)
                    {
                        flag = -1;
                        return flag;
                    }
                }
            }  
        }       
        return flag;
    }    
}
