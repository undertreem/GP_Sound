using System;
using FK_CLI;

namespace GP_Sound_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = WindowSetup(); // ウィンドウ
            var model = ModelSetup(window); // 球モデル
            var countSprite = SpriteSetup(window); // 回数カウント用スプライト
            var timeSprite = SpriteSetup(window); // 経過時間用スプライト
            var heightSprite = SpriteSetup(window); // 高さ用スプライト
            var judgSprite = SpriteSetup(window);//タイミング判定スプライト

            // ウィンドウの描画時間設定
            window.FPS = 60;

            // BGM設定
            var bgm = BGMSetup();

            // SE設定
            var se = SoundEffectSetup();

            // 時間計測用変数
            var time = new fk_Time();
            // 時間計測開始
            time.Start();

            //判定用変数
            var Judg = 0.0;
            var keytime = 0.0;

            for (int count = 0; window.Update() == true; ++count)
            {
                // 経過時間の算出
                double runTime = time.LapTime();

                // 球モデルの位置設定
                //double y = CalcHeight((double)count / 100.0);
                double y = CalcHeight(runTime);
                model.GlMoveTo(0.0, y, 0.0);

                // 各種スプライト更新
                CountUpdate(countSprite, count);
                TimeUpdate(timeSprite, runTime);
                HeightUpdate(heightSprite, y);

                if(y < 0.15)
                {
                    se.StartSound(1);
                    Console.WriteLine($"time = {runTime}");
                    Judg = runTime;
                }

                // xキーを押す
                if (window.GetKeyStatus('x', fk_Switch.DOWN))
                {
                    keytime = runTime;

                    if (Math.Abs(Judg - keytime) < 0.05)//Excellent.
                    {
                        TimeJudg(judgSprite, "Excellent!!");
                        se.StartSound(0);
                    }
                    else if (Math.Abs(Judg - keytime) < 0.1 && Math.Abs(Judg - keytime) > 0.05)//Good.
                    {
                        TimeJudg(judgSprite, "Good.");
                        se.StartSound(0);
                    }
                    else//Bad.
                    {
                        TimeJudg(judgSprite, "Bad...");
                    }

                }

            }

            // BGM と SE の終了処理
            bgm.StopStatus = true;
            se.StopStatus = true;
            bgm.Dispose();
            se.Dispose();
        }

        // ウィンドウ設定メソッド
        static fk_AppWindow WindowSetup()
        {
            var window = new fk_AppWindow();
            window.TrackBallMode = true;
            window.Size = new fk_Dimension(600, 600);
            window.BGColor = new fk_Color(0.6, 0.7, 0.8);
            window.CameraPos = new fk_Vector(0.0, 2.0, 20.0);
            window.CameraFocus = new fk_Vector(0.0, 2.0, 0.0);
            window.Open();
            window.ShowGuide(fk_Guide.GRID_XZ);
            return window;
        }

        // 球モデル設定メソッド
        static fk_Model ModelSetup(fk_AppWindow argWin)
        {
            fk_Material.InitDefault();

            var sphere = new fk_Sphere(8, 0.5);
            var model = new fk_Model();

            model.Shape = sphere;
            model.Material = fk_Material.Red;
            model.SmoothMode = true;
            argWin.Entry(model);
            return model;
        }

        // 各スプライト初期設定メソッド
        static fk_SpriteModel SpriteSetup(fk_AppWindow argWin)
        {
            var sprite = new fk_SpriteModel();
            if (sprite.InitFont("rm1b.ttf") == false)
            {
                Console.WriteLine("Font Init Error");
            }
            argWin.Entry(sprite);
            return sprite;
        }

        // BGM設定用メソッド
        static fk_BGM BGMSetup()
        {
            var bgm = new fk_BGM("bgm.ogg"); // BGM用音源ファイル設定
            bgm.Gain = 0.5; // 音量設定
            bgm.Start(); // BGM再生開始
            return bgm;
        }

        // SE設定用メソッド
        static fk_Sound SoundEffectSetup()
        {
            var se = new fk_Sound(2);

            // ID が0番の音源ファイル設定
            if (se.LoadData(0, "sd.wav") == false)
            {
                Console.WriteLine("SE0 Load Error");
            }

            // ID が1番の音源ファイル設定
            if(se.LoadData(1, "mt.wav") == false)
            {
                Console.WriteLine("SE1 Load Error");
            }

            // SE音量設定
            se.SetGain(0, 1.0);
            se.SetGain(1, 1.0);

            // SE再生準備
            se.Start();
            return se;
        }

        // 球の高さ算出メソッド
        // 引数の小数点部分だけ抽出し、0 と 1 で最下点(y = 0)、0.5 で最上点(y = 4) となる
        static double CalcHeight(double argT)
        {
            double t = argT - Math.Floor(argT);

            return -16.0 * (t * t - t);
        }

        // double型の桁揃え文字列変換
        static string NumFormat(double argNum, string argPreStr)
        {
            string outStr = String.Format("{0, 6}", $"{argNum:F3}");
            return String.Format($"{argPreStr + outStr, 15}");
        }

        // int型の桁揃え文字列変換
        static string NumFormat(int argNum, string argPreStr)
        {
            return String.Format($"{argPreStr + argNum.ToString(), 15}");
        }

        // 回数カウント用スプライト更新メソッド
        static void CountUpdate(fk_SpriteModel argSprite, int argCount)
        {
            argSprite.DrawText(NumFormat(argCount, "frame = "), true);
            argSprite.SetPositionLT(-280, 280);
        }

        // 経過時間用スプライト更新メソッド
        static void TimeUpdate(fk_SpriteModel argSprite, double argTime)
        {
            argSprite.DrawText(NumFormat(argTime, "time = "), true);
            argSprite.SetPositionLT(-280, 250);
        }

        // 高さ表示用スプライト更新メソッド
        static void HeightUpdate(fk_SpriteModel argSprite, double argY)
        {
            argSprite.DrawText(NumFormat(argY, "height = "), true);
            argSprite.SetPositionLT(-280, 220);
        }

        // 判定表示用スプライト更新メソッド
        static void TimeJudg(fk_SpriteModel argSprite, string argY)
        {
            argSprite.DrawText(argY, true);
            argSprite.SetPositionLT(0, 180);
        }
    }
}
