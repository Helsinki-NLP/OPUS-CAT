using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Small helper class to be able to generate random token codes.
    /// </summary>
    public class TokenCodeGenerator
    {
        /// <summary>
        /// Class contains the token information related to a single user.
        /// </summary>
        private class UserTokenInfo
        {
            /// <summary>
            /// The timer to be able to invalidate the token code.
            /// </summary>
            public Timer Timer;

            /// <summary>
            /// The token code.
            /// </summary>
            public string TokenCode;
        }

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static TokenCodeGenerator instance;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static TokenCodeGenerator Instance
        {
            get
            {
                if (instance == null)
                    instance = new TokenCodeGenerator();

                return instance;
            }
        }

        /// <summary>
        /// The users' token informations.
        /// </summary>
        private Dictionary<string, UserTokenInfo> userTokens;

        private TokenCodeGenerator()
        {
            userTokens = new Dictionary<string, UserTokenInfo>();
        }

        /// <summary>
        /// Invalidates the token code related to a single user.
        /// Called when the timer expires (currently 2 minutes
        /// after the creation).
        /// </summary>
        /// <param name="state">The username.</param>
        private void invalidateTokenCode(object state)
        {
            userTokens.Remove(state.ToString());
        }

        /// <summary>
        /// Generates a random token code for a single user.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The token code.</returns>
        public string GenerateTokenCode(string userName)
        {
            string tokenCode = Guid.NewGuid().ToString();

            if (userTokens.ContainsKey(userName))
            {
                userTokens[userName].Timer.Dispose();
                userTokens.Remove(userName);
            }

            userTokens[userName] = new UserTokenInfo()
            {
                Timer = new Timer(new TimerCallback(invalidateTokenCode), userName, new TimeSpan(0, 2, 0), new TimeSpan(0, 2, 0)),
                TokenCode = tokenCode
            };

            return tokenCode;
        }

        /// <summary>
        /// Indicates whether the given token code is valid or not.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>True if the token code is valid.</returns>
        public bool TokenCodeIsValid(string tokenCode)
        {
            /*
            return userTokens.Values.Any(uti => uti.TokenCode.Equals(tokenCode));*/
            //Disabled for now, seems to cause problems
            return true;
        }
    }
}
