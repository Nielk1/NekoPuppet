using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoPuppet
{
    public struct Vector2D
    {
        private float x;
        private float y;

        public Vector2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class CharacterControlInterface
    {
        private EmoteEngineNet.EmotePlayer character;

        public CharacterControlInterface(EmoteEngineNet.EmotePlayer character)
        {
            this.character = character;
        }

        public uint CountDiffTimelines()
        {
            return character.CountDiffTimelines();
        }
        public uint CountMainTimelines()
        {
            return character.CountMainTimelines();
        }
        public uint CountPlayingTimelines()
        {
            return character.CountPlayingTimelines();
        }
        public uint CountVariableFrameAt(uint variableIndex)
        {
            return character.CountVariableFrameAt(variableIndex);
        }
        public uint CountVariables()
        {
            return character.CountVariables();
        }
        public void FadeInTimeline(string label, float frameCount, float easing)
        {
            character.FadeInTimeline(label, frameCount, easing);
        }
        public void FadeOutTimeline(string label, float frameCount, float easing)
        {
            character.FadeOutTimeline(label, frameCount, easing);
        }
        public float GetBustScale()
        {
            return character.GetBustScale();
        }
        public System.Drawing.Color GetColor()
        {
            return System.Drawing.Color.FromArgb((int)character.GetColor());
        }
        //public Vector2D GetCoord()
        //{
        //    float x = 0f;
        //    float y = 0f;
        //    character.GetCoord(ref x, ref y);
        //    return new Vector2D(x, y);
        //}
        public string GetDiffTimelineLabelAt(uint index)
        {
            return character.GetDiffTimelineLabelAt(index);
        }
        public float GetHairScale()
        {
            return character.GetHairScale();
        }
        public string GetMainTimelineLabelAt(uint index)
        {
            return character.GetMainTimelineLabelAt(index);
        }
        public float GetMeshDivisionRatio()
        {
            return character.GetMeshDivisionRatio();
        }
        public float GetPartsScale()
        {
            return character.GetPartsScale();
        }
        public uint GetPlayingTimelineFlagsAt(uint index)
        {
            return character.GetPlayingTimelineFlagsAt(index);
        }
        public string GetPlayingTimelineLabelAt(uint index)
        {
            return character.GetPlayingTimelineLabelAt(index);
        }
        public bool GetQueuing()
        {
            return character.GetQueuing();
        }
        public float GetRot()
        {
            return character.GetRot();
        }
        public float GetScale()
        {
            return character.GetScale();
        }
        public bool GetSmoothing()
        {
            return character.GetSmoothing();
        }
        public float GetTimelineBlendRatio(string label)
        {
            return character.GetTimelineBlendRatio(label);
        }
        public float GetVariable(string var)
        {
            return character.GetVariable(var);
        }
        public string GetVariableFrameLabelAt(uint variableIndex, uint frameIndex)
        {
            return character.GetVariableFrameLabelAt(variableIndex, frameIndex);
        }
        public float GetVariableFrameValueAt(uint variableIndex, uint frameIndex)
        {
            return character.GetVariableFrameValueAt(variableIndex, frameIndex);
        }
        public string GetVariableLabelAt(uint index)
        {
            return character.GetVariableLabelAt(index);
        }
        public void Hide()
        {
            character.Hide();
        }
        public bool IsAnimating()
        {
            return character.IsAnimating();
        }
        public bool IsHidden()
        {
            return character.IsHidden();
        }
        public bool IsLoopTimeline(string label)
        {
            return character.IsLoopTimeline(label);
        }
        public bool IsModified()
        {
            return character.IsModified();
        }
        public bool IsTimelinePlaying(string label)
        {
            return character.IsTimelinePlaying(label);
        }
        public void OffsetCoord(int ofsx, int ofsy)
        {
            character.OffsetCoord(ofsx, ofsy);
        }
        public void OffsetRot(float ofstRot)
        {
            character.OffsetRot(ofstRot);
        }
        public void OffsetScale(float ofstScale)
        {
            character.OffsetScale(ofstScale);
        }
        public void PlayTimeline(string label, EmoteEngineNet.TimelinePlayFlags flag)
        {
            character.PlayTimeline(label, flag);
        }
        public void Progress(float ms)
        {
            character.Progress(ms);
        }
        public void Release()
        {
            character.Release();
        }
        public void Render()
        {
            character.Render();
        }
        public void SetBustScale(float scale)
        {
            character.SetBustScale(scale);
        }
        public void SetColor(System.Drawing.Color color, float frameCount, float easing)
        {
            character.SetColor((uint)color.ToArgb(), frameCount, easing);
        }
        public void SetCoord(float x, float y, float frameCount, float easing)
        {
            character.SetCoord(x, y, frameCount, easing);
        }
        public void SetHairScale(float scale)
        {
            character.SetHairScale(scale);
        }
        public void SetMeshDivisionRatio(float ratio)
        {
            character.SetMeshDivisionRatio(ratio);
        }
        public void SetPartsScale(float scale)
        {
            character.SetPartsScale(scale);
        }
        public void SetQueuing(bool state)
        {
            character.SetQueuing(state);
        }
        public void SetRot(float rot, float frameCount, float easing)
        {
            character.SetRot(rot, frameCount, easing);
        }
        public void SetScale(float scale, float frameCount, float easing)
        {
            character.SetScale(scale, frameCount, easing);
        }
        public void SetSmoothing(bool state)
        {
            character.SetSmoothing(state);
        }
        public void SetTimelineBlendRatio(string label, float value, float frameCount, float easing, bool stopWhenBlendDone)
        {
            character.SetTimelineBlendRatio(label, value, frameCount, easing, stopWhenBlendDone);
        }
        public void SetVariable(string var, float value, float frameCount, float easing)
        {
            character.SetVariable(var, value, frameCount, easing);
        }
        public void SetVariables(IDictionary<string, float> table, float time, float easing)
        {
            character.SetVariables(table, time, easing);
        }
        public void Show()
        {
            character.Show();
        }
        public void Skip()
        {
            character.Skip();
        }
        public void StartWind(float start, float goal, float speed, float powMin, float powMax)
        {
            character.StartWind(start, goal, speed, powMin, powMax);
        }
        public void StopTimeline(string label)
        {
            character.StopTimeline(label);
        }
        public void StopWind()
        {
            character.StopWind();
        }
    }
}