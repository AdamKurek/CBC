using CBC.Shared;
using ConcurrentLinkedListQueue;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using static VideoChatHub;

namespace CBC.Server
{
    internal class TalkersQueues
    {
        int AgeMinimum;
        internal TalkersQueues(int AgeMin, int AgeMax)
        {
            AgeMinimum = AgeMin;
            var Fields = AgeMax - AgeMin;
            Males = new ConcurrentLinkedListQueue<InQueueStatus>[Fields];
            Females = new ConcurrentLinkedListQueue<InQueueStatus>[Fields];
            for (int i = 0; i < Fields; i++)
            {
                Males[i] = new ConcurrentLinkedListQueue<InQueueStatus>();
                Females[i] = new ConcurrentLinkedListQueue<InQueueStatus>();
            }
        }

        internal void Push(int Age, bool Female, InQueueStatus user)
        {
            if(Female)
            {
                Females[Age - AgeMinimum].Enqueue(user);
                return;
            }
            Males[Age - AgeMinimum].Enqueue(user);
            Console.WriteLine(Males[Age - AgeMinimum].Count() + "malesow");
            //Females[Age].Enqueue(user);
        }

        internal string GetId(UserPreferences requirements, QueueUser user, string ConnID)
        {
            int avr = ((requirements.MinAge + requirements.MaxAge) / 2) - AgeMinimum;

            for (int i = requirements.MaxAge - AgeMinimum; i > requirements.MinAge - AgeMinimum; )
            {
                --i;
                //if (requirements.AcceptMale)
                //{
                //    if (r < requirements.MaxAge && Males[r].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //    if (l >= 0 && Males[l].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //}
                //if (requirements.AcceptFemale)
                //{
                //    if (r < requirements.MaxAge && Females[r].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //    if (l >= 0 && Females[l].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //}
                Console.WriteLine(2);

                if (requirements.AcceptFemale)
                {
                    var gotVal = Females[i].GetFirstUserWithCondition(
                        ueueUser =>
                        ueueUser.preferences.MinAge <= user.Age &&
                        ueueUser.preferences.MaxAge >= user.Age &&
                        (user.IsFemale ? ueueUser.preferences.AcceptFemale : ueueUser.preferences.AcceptMale) &&
                        ueueUser.preferences.ConnectionId != ConnID);
                    if (gotVal is object)
                    {
                        return gotVal.preferences.ConnectionId;
                    }
                }
                if (requirements.AcceptMale) {
                    var gotVal = Males[i].GetFirstUserWithCondition(
                        ueueUser =>
                        ueueUser.preferences.MinAge <= user.Age &&
                        ueueUser.preferences.MaxAge >= user.Age &&
                        (user.IsFemale ? ueueUser.preferences.AcceptFemale : ueueUser.preferences.AcceptMale) &&
                        ueueUser.preferences.ConnectionId != ConnID);
                    if (gotVal is object)
                    {
                        return gotVal.preferences.ConnectionId;
                    }
                }
            }
            Console.WriteLine(3);

            return null;
        }

        internal bool RemoveUser(QueueUser user, string ConnID)
        {
            if (user.IsFemale) {
                return Females[user.Age - AgeMinimum].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.preferences.ConnectionId == ConnID)) is object;
            }
            return Males[user.Age - AgeMinimum].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.preferences.ConnectionId == ConnID)) is object;
        }


        internal ConcurrentLinkedListQueue<InQueueStatus>[] Males;
        internal ConcurrentLinkedListQueue<InQueueStatus>[] Females;
    }


    //internal static class ConcurrentQueueExtention
    //{
    //    public static bool GetFirstUserWithCondition(this LinkedList<UserPreferences> que, Func<UserPreferences, bool> condition,ref string ConnId)
    //    {
    //        try { 
    //            LinkedListNode<UserPreferences>? node = que.First; // Start with the first node
    //            while (node is object)
    //            {
    //                if (condition(node.Value))
    //                {
    //                    que.Remove(node); // Remove the current node
    //                    ConnId = node.Value.ConnectionId; // Save the connection id
    //                    return true;
    //                }
    //                node = node.Next; // Save the next node
    //            }
    //        }
    //        catch (Exception e) { 
    //            return false;
    //        }
    //        return false;

    //    }
    //}
}
