using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class Waiting : ABaseMode
    {
        public Waiting(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.Waiting;
            ReactionSCEmployers += new ReactionSCEmployersHandler(Waiting_ReactionSCEmployers);
        } // Waiting (constructor)
        void Waiting_ReactionSCEmployers(object sender, RefEmployer employer)
        {
            JumpTo(new ChoiseWork(SS, this, employer.IDD));
        } // Waiting_ReactionSCEmployers
        internal override ABaseMode Init()
        {
            if (FEmployer != null)
            {
                if (!Logout())
                {
                    return Negative(SS.ExcStr);
                }
            }
            FEmployer = null;
            return Positive();
        } // Init
    }
}
