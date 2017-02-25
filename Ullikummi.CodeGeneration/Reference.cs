using System;
using System.Linq;

namespace Ullikummi.FluentApi
{
    internal class Identity
    {

    }

    internal class IdentityBuilder :
            IdentityBuilderTransitions.Ialpha,
            IdentityBuilderTransitions.Ibeta,
            IdentityBuilderTransitions.Igamma,
            IdentityBuilderTransitions.Iempty,
            IdentityBuilderTransitions.IalphaBeta,
            IdentityBuilderTransitions.IalphaGamma,
            IdentityBuilderTransitions.IbetaGamma,
            IdentityBuilderTransitions.IalphaBetaGamma
    {
        protected virtual void Alpha(string name, string surname)
        {

        }

        protected virtual void Beta(string address)
        {

        }

        protected virtual void Gamma(string phoneNumber)
        {

        }

        protected virtual Identity Build()
        {
            return default(Identity);
        }

        protected virtual void ConfigureInternal()
        {

        }

        public IdentityBuilderTransitions.IalphaBetaGamma Configure()
        {
            ConfigureInternal();
            return this;
        }

        IdentityBuilderTransitions.Ibeta IdentityBuilderTransitions.IalphaBeta.Alpha(string name, string surname)
        {
            Alpha(name, surname);
            return this;
        }

        IdentityBuilderTransitions.Igamma IdentityBuilderTransitions.IalphaGamma.Alpha(string name, string surname)
        {
            Alpha(name, surname);
            return this;
        }

        IdentityBuilderTransitions.IbetaGamma IdentityBuilderTransitions.IalphaBetaGamma.Alpha(string name, string surname)
        {
            Alpha(name, surname);
            return this;
        }

        IdentityBuilderTransitions.Iempty IdentityBuilderTransitions.Ialpha.Alpha(string name, string surname)
        {
            Alpha(name, surname);
            return this;
        }

        IdentityBuilderTransitions.IalphaGamma IdentityBuilderTransitions.IalphaBetaGamma.Beta(string address)
        {
            Beta(address);
            return this;
        }

        IdentityBuilderTransitions.Igamma IdentityBuilderTransitions.IbetaGamma.Beta(string address)
        {
            Beta(address);
            return this;
        }

        IdentityBuilderTransitions.Ialpha IdentityBuilderTransitions.IalphaBeta.Beta(string address)
        {
            Beta(address);
            return this;
        }

        IdentityBuilderTransitions.Iempty IdentityBuilderTransitions.Ibeta.Beta(string address)
        {
            Beta(address);
            return this;
        }

        Identity IdentityBuilderTransitions.IalphaBetaGamma.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.IbetaGamma.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.IalphaGamma.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.Iempty.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.Ibeta.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.IalphaBeta.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.Igamma.Build()
        {
            return Build();
        }

        Identity IdentityBuilderTransitions.Ialpha.Build()
        {
            return Build();
        }

        IdentityBuilderTransitions.IalphaBeta IdentityBuilderTransitions.IalphaBetaGamma.Gamma(string phoneNumber)
        {
            Gamma(phoneNumber);
            return this;
        }

        IdentityBuilderTransitions.Ibeta IdentityBuilderTransitions.IbetaGamma.Gamma(string phoneNumber)
        {
            Gamma(phoneNumber);
            return this;
        }

        IdentityBuilderTransitions.Ialpha IdentityBuilderTransitions.IalphaGamma.Gamma(string phoneNumber)
        {
            Gamma(phoneNumber);
            return this;
        }

        IdentityBuilderTransitions.Iempty IdentityBuilderTransitions.Igamma.Gamma(string phoneNumber)
        {
            Gamma(phoneNumber);
            return this;
        }
    }

    internal class IdentityBuilderTransitions
    {
        public interface Ialpha
        {
            Iempty Alpha(string name, string surname);
            Identity Build();
        }

        public interface Ibeta
        {
            Iempty Beta(string address);
            Identity Build();
        }

        public interface Igamma
        {
            Iempty Gamma(string phoneNumber);
            Identity Build();
        }

        public interface Iempty
        {
            Identity Build();
        }

        public interface IalphaBeta
        {
            Ibeta Alpha(string name, string surname);
            Ialpha Beta(string address);
            Identity Build();
        }

        public interface IalphaGamma
        {
            Igamma Alpha(string name, string surname);
            Ialpha Gamma(string phoneNumber);
            Identity Build();
        }

        public interface IbetaGamma
        {
            Igamma Beta(string address);
            Ibeta Gamma(string phoneNumber);
            Identity Build();
        }

        public interface IalphaBetaGamma
        {
            IbetaGamma Alpha(string name, string surname);
            IalphaGamma Beta(string address);
            IalphaBeta Gamma(string phoneNumber);
            Identity Build();
        }
    }
}
