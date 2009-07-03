﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnwood.SmtpServer.Extensions.Auth
{
    public class CramMd5Mechanism : AuthMechanism
    {
        public override string Identifier
        {
            get { return "CRAM-MD5"; }
        }

        public override AuthMechanismProcessor CreateAuthMechanismProcessor(ConnectionProcessor connectionProcessor)
        {
            return new CramMd5MechanismProcessor(connectionProcessor);
        }
    }

    public class CramMd5MechanismProcessor : AuthMechanismProcessor
    {
        public CramMd5MechanismProcessor(ConnectionProcessor processor)
        {
            ConnectionProcessor = processor;
        }

        protected ConnectionProcessor ConnectionProcessor { get; set; }
        private Random _random = new Random();

        enum States
        {
            Initial,
            AwaitingResponse
        }

        private string _challenge;


        public override AuthMechanismProcessorStatus ProcessResponse(string data)
        {
            if (_challenge == null)
            {
                StringBuilder challenge = new StringBuilder();
                challenge.Append(_random.Next(Int16.MaxValue));
                challenge.Append(".");
                challenge.Append(DateTime.Now.Ticks.ToString());
                challenge.Append("@");
                challenge.Append(ConnectionProcessor.Server.Behaviour.DomainName);
                _challenge = challenge.ToString();

                string base64Challenge = Convert.ToBase64String(Encoding.ASCII.GetBytes(challenge.ToString()));
                ConnectionProcessor.WriteResponse(new SmtpResponse(StandardSmtpResponseCode.AuthenticationContinue,
                                                                   base64Challenge));
                return AuthMechanismProcessorStatus.Continue;
            } else
            {
                string response = DecodeBase64(data);
                string[] responseparts = response.Split(' ');

                if (responseparts.Length != 2)
                {
                    throw new SmtpServerException(new SmtpResponse(StandardSmtpResponseCode.AuthenticationFailure,
                                               "Response in incorrect format - should be USERNAME RESPONSE"));
                }

                string username = responseparts[0];
                string hash = responseparts[1];

                return AuthMechanismProcessorStatus.Success;
            }
        }

        private static string DecodeBase64(string data)
        {
            try
            {
                return Encoding.ASCII.GetString(Convert.FromBase64String(data));
            }
            catch (FormatException)
            {
                throw new SmtpServerException(new SmtpResponse(StandardSmtpResponseCode.AuthenticationFailure,
                                                               "Bad Base64 data"));
            }
        }
    }
}
