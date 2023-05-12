using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public static class UnitaskTokenContainer
{
    public enum TokenType { SCENE, GROUP, OBJECT, GLOBAL, NONE }

    public struct TokenData
    {
        public CancellationToken Token { get; private set; }
        public int TokenID { get; private set; }
        
        public TokenData(CancellationToken token, int handleId)
        {
            Token = token;
            TokenID = handleId;
        }
    }
    private class TokenHandler : IDisposable
    {
        private CancellationTokenSource source;
        public int TokenID { get; private set; }

        public TokenData TokenData => new TokenData(source.Token, TokenID);


        public TokenHandler()
        {
            source = new CancellationTokenSource();
            TokenID = source.GetHashCode();
        }

        public void Dispose()
        {
            if(source != null) {
                source.Cancel();
                source.Dispose();
                source = null;
            }
        }
    }
    private class SceneTokenHandler
    {
        private TokenHandler sceneToken;
        private Dictionary<string, TokenHandler> groupTokens = new Dictionary<string, TokenHandler>();
        private Dictionary<int, TokenHandler> objectTokens = new Dictionary<int, TokenHandler>();

        //Create TokenSource and return TokenData
        #region GetToken Functions
        public TokenData GetSceneToken()
        {
            sceneToken ??= new TokenHandler();
            return sceneToken.TokenData;
        }

        public TokenData GetObjectToken()
        {
            var newToken = new TokenHandler();
            objectTokens.Add(newToken.TokenID, newToken);

            return newToken.TokenData;
        }

        public TokenData GetGroupToken(string groupKey)
        {

            if(!groupTokens.TryGetValue(groupKey, out var tokenInfo)) {
                tokenInfo = new TokenHandler();
                groupTokens[groupKey] = tokenInfo;
            }
            return tokenInfo.TokenData;
        }
        #endregion

        //Cancellation and Disposable
        #region CancelToken Function
        public bool Cancel(int tokenID)
        {
            return RemoveToken(tokenID);
        }

        public bool Cancel(in TokenData tokenData)
        {
            return Cancel(tokenData.TokenID);
        }

        public bool Cancel(string key)
        {
            if(groupTokens.TryGetValue(key, out var tokenHandler)) {
                tokenHandler.Dispose();
                groupTokens.Remove(key);
                return true;
            }
            return false;
        }
        #endregion

        //Clear TokenSource
        #region Clear Token
        public void ClearToken()
        {
            ClearToken(TokenType.SCENE);
            ClearToken(TokenType.OBJECT);
            ClearToken(TokenType.GROUP);
        }

        public void ClearToken(TokenType tokenType)
        {
            switch(tokenType) {
                case TokenType.SCENE: {
                    sceneToken?.Dispose();
                    break;
                }
                case TokenType.OBJECT: {
                    foreach(var item in objectTokens) {
                        item.Value.Dispose();
                    }
                    objectTokens.Clear();
                    break;
                }
                case TokenType.GROUP: {
                    foreach(var item in groupTokens) {
                        item.Value.Dispose();
                    }
                    groupTokens.Clear();
                    break;
                }
                default: {
                    break;
                }
            }
        }
        #endregion

        /**
         *  @brief  토큰을 삭제한다.
         *  @param  tokenID : 삭제하려는 token id
         *  @return true : token이 있는 경우, false : token이 없는 경우
         */
        private bool RemoveToken(int tokenID)
        {
            //check scene token
            if(sceneToken != null && sceneToken.TokenID == tokenID) {
                sceneToken.Dispose();
                sceneToken = null;
                return true;
            }

            //find object token
            if(objectTokens.TryGetValue(tokenID, out var getInfo)) {
                getInfo.Dispose();
                objectTokens.Remove(tokenID);
                return true;
            }

            //find group token
            foreach(var group in groupTokens) {
                if(group.Value.TokenID == tokenID) {
                    group.Value.Dispose();
                    groupTokens.Remove(group.Key);
                    return true;
                }
            }
            return false;
        }

        /**
         *  @brief  tokenID와 동일한 Token 정보가 있는지 확인한다.
         *  @param  tokenID : 찾으려는 token id
         *  @return 찾은 tokenInfo
         */
        private TokenHandler GetTokenInfo(int tokenID)
        {
            //check scene token
            if(sceneToken != null && sceneToken.TokenID == tokenID) {
                return sceneToken;
            }

            //find object token
            if(objectTokens.TryGetValue(tokenID, out var getInfo)) {
                return getInfo;
            }

            //find group token
            foreach(var group in groupTokens) {
                if(group.Value.TokenID == tokenID) {
                    return group.Value;
                }
            }
            return null;
        }
    }

    private static Dictionary<string, TokenHandler> globalTokens = new Dictionary<string, TokenHandler>();
    private static Dictionary<int, SceneTokenHandler> sceneTokenHandlers = new Dictionary<int, SceneTokenHandler>();

    //called when the project start
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }


    private static void SceneManager_sceneUnloaded(Scene arg0)
    {
        if(sceneTokenHandlers.TryGetValue(arg0.handle, out var handler)) {
            handler.ClearToken();
            sceneTokenHandlers.Remove(arg0.handle);
        }
    }


    //Create TokenSource and return TokenData
    public static TokenData GetGlobalToken(string globalKey)
    {
        if(globalTokens.TryGetValue(globalKey, out var tokenHandler)){
            return tokenHandler.TokenData;
        }
        else {
            var newTokenHandler = new TokenHandler();
            globalTokens[globalKey] = newTokenHandler;
            return newTokenHandler.TokenData;
        }
    }

    public static TokenData GetSceneToken(Scene? targetScene = null)
    {
        int sceneHandle = GetTargetScene(targetScene).handle;
        var sceneTokenHandler = GetSceneTokenHandler(sceneHandle, true);
        return sceneTokenHandler.GetSceneToken();
    }

    public static TokenData GetGroupToken(string groupKey, Scene? targetScene = null)
    {
        int sceneHandle = GetTargetScene(targetScene).handle;
        var sceneTokenHandler = GetSceneTokenHandler(sceneHandle, true);
        return sceneTokenHandler.GetGroupToken(groupKey);
    }

    public static TokenData GetObjectToken(Scene? targetScene = null)
    {
        int sceneHandle = GetTargetScene(targetScene).handle;
        var sceneTokenHandler = GetSceneTokenHandler(sceneHandle, true);
        return sceneTokenHandler.GetObjectToken();
    }


    public static bool Cancel(string key)
    {
        //현재 씬에서 찾기
        var activeScene = SceneManager.GetActiveScene();
        int sceneHandle = activeScene.handle;

        CancellationSceneToken(sceneHandle, key);

        //global token에서 찾기
        if(globalTokens.TryGetValue(key, out var handler)) {
            handler.Dispose();
            globalTokens.Remove(key);
            return true;
        }
        return false;
    }

    public static bool Cancel(int tokenID)
    {
        var activeScene = SceneManager.GetActiveScene();
        int sceneHandle = activeScene.handle;

        //현재 씬에서 토큰 찾기
        if(CancellationSceneToken(sceneHandle, tokenID)) {
            return true;
        }

        //global token에서 찾기
        foreach(var item in globalTokens) {
            var tokenHandler = item.Value;
            if(tokenHandler.TokenID == tokenID) {
                tokenHandler.Dispose();
                globalTokens.Remove(item.Key);
                return true;
            }
        }

        return false;
    }

    public static bool Cancel(in TokenData tokenData)
    {
        return Cancel(tokenData.TokenID);
    }


    private static bool CancellationSceneToken(int sceneHandle, int tokenID)
    {
        var handler = GetSceneTokenHandler(sceneHandle);
        if(handler == null) {
            return false;
        }
        return handler.Cancel(tokenID);
    }

    private static bool CancellationSceneToken(int sceneHandle, string key)
    {
        var handler = GetSceneTokenHandler(sceneHandle);
        if(handler == null) {
            return false;
        }
        return handler.Cancel(key);
    }


    private static SceneTokenHandler GetSceneTokenHandler(int sceneHandle, bool isCreate = false)
    {
        if(sceneTokenHandlers.TryGetValue(sceneHandle, out var tokenHandler)) {
            return tokenHandler;
        }
        else if(isCreate) {
            var newSceneTokenHandler = new SceneTokenHandler();
            sceneTokenHandlers[sceneHandle] = newSceneTokenHandler;
            return newSceneTokenHandler;
        }

        return null;
    }

    private static Scene GetTargetScene(Scene? scene)
    {
        if(scene == null || scene.Value.buildIndex == -1) {
            var activeScene = SceneManager.GetActiveScene();
            if(activeScene.buildIndex == -1) {
                throw new InvalidOperationException("not found target scene data");
            }
            return activeScene;
        }

        return scene.Value;
    }
}
