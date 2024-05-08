using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager<T>
{
    public Dictionary<int, List<Command<T>>> m_commandDic = new Dictionary<int, List<Command<T>>>();
    private List<List<Command<T>>> m_cmdList = new List<List<Command<T>>>();

    public void AddCommand(Command<T> cmd)
    {
        int cmdType = cmd.GetCmdType();
        if (m_commandDic.TryGetValue(cmdType, out var list) == false)
        {
            list = new List<Command<T>>();
            m_commandDic.Add(cmdType, list);
        }

        list.Add(cmd);
    }

    public void ClearCmd(int type, Predicate<Command<T>> match = null)
    {
        if (m_commandDic.TryGetValue(type, out var cmdList))
        {
            foreach (var cmd in cmdList)
            {
                if (match.IsNotNull() && match(cmd) == false)
                    continue;

                cmd.Exit();
            }
        }
    }

    public void ClearAllCmd()
    {
        foreach (var dic in m_commandDic)
        {
            foreach (var cmd in dic.Value)
            {
                cmd.Exit();
            }
        }
    }

    public R GetCmd<R>(int type) where R : Command<T>
    {
        if (m_commandDic.TryGetValue(type, out var cmdList))
        {
            foreach (var cmd in cmdList)
            {
                if (cmd.IsExit())
                    continue;

                return (R)cmd;
            }
        }

        return null;
    }

    public void Update()
    {
        m_cmdList.Clear();
        m_cmdList.AddRange(m_commandDic.Values);

        foreach (var cmdList in m_cmdList)
        {
            for (int i = 0; i < cmdList.Count; i++)
            {
                Command<T> cmd = cmdList[i];

                try
                {
                    if (cmd.IsExit())
                    {
                        cmdList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        cmd.Update();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Command update failed {e}");
                    cmdList.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}