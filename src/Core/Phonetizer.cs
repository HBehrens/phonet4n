/*
 *  This file is part of phonet4n.
 * 
 *  Copyright 2008 Heiko Behrens (HeikoBehrens a t gmx de)
 *
 *  phonet4n is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  phonet4n is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with phonet4n.
 *  If not, see <http://www.gnu.org/licenses/>.
 */
 
using System;
using System.Collections.Generic;

namespace phonet4n.Core
{
    public class PhonetTracer
    {
        public static void TraceInfo (String[] rules, String  text, int n, String err_text) {

            String s,s2,s3;
            s  = (rules[n] == null) ?  "(NULL)" : rules[n];
            s2 = (rules[n+1] == null) ? "(NULL)" : rules[n+1];
            s3 = (rules[n+2] == null) ? "(NULL)" : rules[n+2];
            
            Console.WriteLine(text + " " + ((n/3)+1) + ":  \"" + s + " \" " + s2 + " \" " + s3 + " \" " + err_text);
            
        }
    }
    
    public class Functions
    {
        public static int strchr(string buffer, int fromIndex, int ch)
        {
        	return buffer.IndexOf((char)ch, fromIndex);
        }

        public static int strchr(char[] buffer, int fromIndex, int ch)
        {
        	return strchr(new String(buffer), fromIndex, ch);
        }

        public static int[] Slice(int[,] source, int index)
        {
            int dim = 1;
            int[] result = new int[source.GetLength(dim)];
            for(int i = 0; i < source.GetLength(dim); i++)
                result[i] = source[index, i];

            return result;
        }
    }
    
	public class Phonetizer
	{
		private String[] phonetRules = null;
		public String[] Rules 
		{
			get {return phonetRules;}
			set 
			{
				phonetRules = value;
				InitRulesHash();
			}
		}
		
		public void SetLangage(string lang)
		{
			Rules = RuleLoader.LoadFromLanguage(lang);
		}
		
	    public static int HASH_COUNT = 65536;
	    
	    /**
	     * list of "normal" letters.
	     *
	     */
	    public static readonly String letters_a_to_z  = "abcdefghijklmnopqrstuvwxyz";
	
	    /**
	     * list of "normal" letters.
	     *
	     */
	    public static readonly String letters_A_to_Z  = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	
	    /**
	     * list of umlauts.
	     *
	     */
	    public static readonly String umlaut_lower    = "\u00e0\u00e1\u00e2\u00e3\u00e5\u00e4\u00e6\u00e7\u00f0\u00e8\u00e9\u00ea\u00eb\u00ec\u00ed\u00ee\u00ef\u00f1\u00f2\u00f3\u00f4\u00f5\u00f8\u009c\u00f6\u009a\u00df\u00fe\u00f9\u00fa\u00fb\u00fc\u00fd\u00ff";
	
	    /**
	     * list of umlauts.
	     *
	     */
	    public static readonly String umlaut_upper    = "\u00c0\u00c1\u00c2\u00c3\u00c5\u00c4\u00c6\u00c7\u00d0\u00c8\u00c9\u00ca\u00cb\u00cc\u00cd\u00ce\u00cf\u00d1\u00d2\u00d3\u00d4\u00d5\u00d8\u008c\u00d6\u008a\u00df\u00de\u00d9\u00da\u00db\u00dc\u00dd\u009f";
	    
	    static readonly bool doTrace = false;
        static readonly bool doCheckRules = false;
        public static String PHONET_END = "";
		private int[] phonetHash;
    	private int[,] phonetHash1, phonetHash2;
    	private static int[]  alphaPos = new int[HASH_COUNT];
	    private static char[] upperChar = new char[HASH_COUNT];
	    private static int[]  isLetter  = new int[HASH_COUNT];
    	
    	static Phonetizer()
    	{
	        //  generate arrays "alpha_pos", "upperchar" and "isletter"
	        for (int i=0; i< HASH_COUNT; i++) {
	            alphaPos[i] = 0;
	            isLetter[i] = 0;
	            upperChar[i] = (char) i;
	        }
	        
	        // init letter indexing arrays
	        String lettersLower, lettersUpper;
	        int ip;
	        for (int k=-1; k<1; k++) {
	            
	            if (k == -1) {
	                // German and international umlauts
	                lettersLower = umlaut_lower;
	                lettersUpper = umlaut_upper;
	
	            } else {
	                // "normal" letters ('a'-'z' and 'A'-'Z')
	                lettersLower = letters_a_to_z;
	                lettersUpper = letters_A_to_Z;
	
	            }
	            
	            for (int i=0; i < lettersLower.Length ; i++) {
	                
	                if (k == -1)
	                    ip = k;
	                else
	                    ip = i;
	
	                alphaPos [lettersLower[i]] = ip + 2;
	                alphaPos [lettersUpper[i]] = ip + 2;
	                isLetter [lettersLower[i]] = 1;
	                isLetter [lettersUpper[i]] = 1;
	                upperChar [lettersLower[i]] = lettersUpper[i];
	                upperChar [lettersUpper[i]] = lettersUpper[i];
	            } // for i
	        
	        } // for k
    	}
		
		private void InitRulesHash()
        {
	        // create hash arrays
	        phonetHash = new int[HASH_COUNT];
	        phonetHash1 = new int[26, 28];
	        phonetHash2 = new int[26, 28];			
	        
	        for (int i=0; i < HASH_COUNT; i++)
	            phonetHash[i] = -1;

	        for (int i=0; i<26; i++) {
	            for (int j=0; j<28; j++) {
	                phonetHash1[i, j] = -1;
	                phonetHash2[i, j] = -1;
	            }
	        }

	        // hash the rules
	        int k;
	        
	        String nextRule;
	
	        for (int i=0; phonetRules[i] != PHONET_END; i += 3) { // XXX +=2

                nextRule = phonetRules[i];
	
	            if(nextRule == null) {
	                continue;
                    }
	
	
	            String nextRuleCharArr = nextRule + "\0";
	            
	            //  calculate first hash value
	            k =  nextRuleCharArr[0];
	            if (phonetHash[k] < 0 && (phonetRules[i+1] != null  ||  phonetRules[i+2] != null)) { // XXX: letztes raus
	                phonetHash[k] = i;
	            }
	
	            if(k==0 || alphaPos[k] < 2) {
	                continue;
	            }
	
	            // calculate second hash values
	            int _k = k = alphaPos[k];
	            
	            int nextRuleIdx=1;
	            if (nextRuleCharArr[nextRuleIdx] == '(') {
	                nextRuleIdx++;
	            } else if (nextRuleCharArr[nextRuleIdx] == '\0') {
	                nextRuleCharArr = " \0";
	                nextRuleIdx = 0;
	            } else {
	            	nextRuleCharArr = nextRuleCharArr[nextRuleIdx] + "\0";
	                nextRuleIdx = 0;
	            }
	            
	            while (nextRuleCharArr[nextRuleIdx] != '\0'  &&  nextRuleCharArr[nextRuleIdx] != ')') {
	                
	                k = alphaPos [nextRuleCharArr[nextRuleIdx]];
	                
	                if (k > 0) {
	                    
	                    // add hash value for this letter
	                    if (phonetHash1[_k-2, k] < 0) {
	                        phonetHash1[_k-2, k] = i;
	                        phonetHash2[_k-2, k] = i;
	                    }
	                    
	                    if (phonetHash2[_k-2, k] >= i - 30) {
	                        phonetHash2[_k-2, k] = i;
	                    } else {
	                        k = -1;
	                    }
	                }
	                
	                if (k <= 0) {
	                    // add hash value for all letters
	                    if (phonetHash1[_k-2, 0] < 0) {
	                        phonetHash1[_k-2, 0] = i;
	                    }
	                    phonetHash2[_k-2, 0] = i;
	                }
	                nextRuleIdx++;
	                
	            } // end while
	            
	        } // end for phonetRules...
		}

        public String Phonetize(String srcStr)
        {
            int  k0,n0,p0,z0;
            int  start1=0,end1=0,start2=0,end2=0;
            int  start3=0,end3=0,start4=0,end4=0;
            
            int mode = 1; //we only allow the first rule set

            int[] p_hash1, p_hash2;
            char c0=' ';

            if (srcStr == null) {
                throw new Exception("Error: wrong arguments");
            }

            // toUppercase workaround
            char[] srcUpperStr = (srcStr.ToUpper() + "\0").ToCharArray();
            
            if (doTrace) {
                Console.WriteLine("phonetic conversion for  :  \"" + srcUpperStr + "\"");
            }
            
            //  check srcStr
            int resultLen = 255;
            char[] result = new char[resultLen];

            int srcStrIdx = 0;
            int resultIdx = 0;
            int z = 0;
            char nextCurrentChar;
            int n;

            while ((nextCurrentChar = srcUpperStr[srcStrIdx]) != '\0') {

                if (doTrace) {
                    Console.WriteLine("check position "+resultIdx+":  src = \""+srcUpperStr.ToString().Substring(srcStrIdx, srcUpperStr.Length-srcStrIdx-1)+"\",");
                    String tmpDest = new String(result);
                    Console.WriteLine("  dest = [" + tmpDest.Substring(0, tmpDest.IndexOf('\0')) + "]");

                }
                
                n = alphaPos[nextCurrentChar];

                if (n >= 2) {
                    
                    int _n = n;
                    p_hash1 = Functions.Slice(phonetHash1, n-2);
                    p_hash2 = Functions.Slice(phonetHash2, n-2);
                    
                    n = alphaPos[ srcUpperStr[srcStrIdx+1]];
                    start1 = p_hash1[n];
                    start2 = p_hash1[0];
                    end1 = p_hash2[n];
                    end2 = p_hash2[0];
                    
                    // preserve rule priorities
                    if (start2 >= 0 && (start1 < 0  ||  start2 < start1)) {
                        n = start1;
                        start1 = start2;
                        start2 = n;
                        n = end1;
                        end1 = end2;
                        end2 = n;
                    }
                    
                    if (end1 >= start2  &&  start2 >= 0) {
                        if (end2 > end1) {
                            end1 = end2;
                        }
                        start2 = -1;
                        end2 = -1;
                    }
                } else {
                    n = phonetHash[ nextCurrentChar];
                    start1 = n;
                    end1 = 10000;
                    start2 = -1;
                    end2 = -1;
                } // end if n >= 2
                
                n = start1;
                z0 = 0;

                if (n >= 0) {
                    
                    //  check rules for this char
                    while (phonetRules[n] == null  || phonetRules[n].Length == 0  ||  phonetRules[n][0] == nextCurrentChar ) {

                        if (n > end1) {
                            
                            if (start2 > 0) {
                                n = start2;
                                start1 = start2;  start2 = -1;
                                end1 = end2;  end2 = -1;
                                continue;
                            }
                            break;
                        } // if n > end1

                        if (phonetRules[n] == null  ||  phonetRules[n+mode] == null) {  // XXX n+1
                            //no conversion rule available
                            n += 3; // XXX += 2
                            continue;
                        }

                        if (doTrace) {
                            PhonetTracer.TraceInfo(phonetRules, "> rule no.", n, "is being checked");
                        }
                        
                        //  check whole string
                        int numMatchLetters = 1;   //  no. of matching letters
                        int rulePriority = 5;   //  default priority
                        string nextRuleCharArr = phonetRules[n] + "\0";
                        int nextRuleIdx = 1;     // needed by "*(s-1)" below

                        while (nextRuleCharArr[nextRuleIdx] != '\0'  &&
                                srcUpperStr[srcStrIdx+numMatchLetters] == nextRuleCharArr[nextRuleIdx] &&
                                !Char.IsDigit(nextRuleCharArr[nextRuleIdx]) &&
                                "(-<^$".IndexOf(nextRuleCharArr[nextRuleIdx]) == -1) { //strchr ("(-<^$", *s) == NULL) {
                            numMatchLetters++;
                            nextRuleIdx++;
                        }
                  
                        if (doCheckRules) {
                            
                            //  we do "check_rules"
                            while (nextRuleCharArr[nextRuleIdx] != '\0'  &&  srcUpperStr[srcStrIdx+numMatchLetters] == nextRuleCharArr[nextRuleIdx]) {
                                numMatchLetters++;
                                nextRuleIdx++;
                            }
                        }
                  
                        if (nextRuleCharArr[nextRuleIdx] == '(') {
                            
                            //  check an array of letters
                            if ( (isLetter[ srcUpperStr[srcStrIdx+numMatchLetters]] != 0) && (Functions.strchr(nextRuleCharArr, nextRuleIdx+1, srcUpperStr[srcStrIdx+numMatchLetters]) != -1) ) {
                                
                                numMatchLetters++;
                                while (nextRuleCharArr[nextRuleIdx] != '\0'  &&  nextRuleCharArr[nextRuleIdx] != ')') {
                                    nextRuleIdx++;
                                }
                                if (nextRuleCharArr[nextRuleIdx] == ')') {
                                    nextRuleIdx++;
                                }
                            }
                        } // end if *s == '(' 

                        p0 =  nextRuleCharArr[nextRuleIdx];
                        k0 = numMatchLetters;
                        while (nextRuleCharArr[nextRuleIdx] == '-'  &&  numMatchLetters > 1) {
                            numMatchLetters--;
                            nextRuleIdx++;
                        }
                        if (nextRuleCharArr[nextRuleIdx] == '<') {
                            nextRuleIdx++;
                        }
                        if (Char.IsDigit(nextRuleCharArr[nextRuleIdx] )) {
                            //  read priority
                            rulePriority = nextRuleCharArr[nextRuleIdx] - '0';
                            nextRuleIdx++;
                        }
                        
                        if (nextRuleCharArr[nextRuleIdx] == '^'  &&  nextRuleCharArr[nextRuleIdx+1] == '^') {
                            nextRuleIdx++;
                            if (doCheckRules  &&  isLetter [ srcUpperStr[srcStrIdx+k0]] == 0) {
                                //  we do "check_rules"
                                nextRuleIdx = nextRuleIdx-2;
                            }
                        }
                        
                        if ( nextRuleCharArr[nextRuleIdx] == '\0'  ||
                             (nextRuleCharArr[nextRuleIdx] == '^'  &&
                             (srcStrIdx == 0  ||  isLetter[srcUpperStr[srcStrIdx-1]] == 0) &&
                             (nextRuleCharArr[nextRuleIdx+1] != '$' ||
                             (isLetter[ srcUpperStr[srcStrIdx+k0]] == 0 &&  srcUpperStr[srcStrIdx+k0] != '.'))) ||
                             (nextRuleCharArr[nextRuleIdx] == '$'  &&  srcStrIdx > 0  &&  isLetter[ srcUpperStr[srcStrIdx-1]] != 0 &&
                             (isLetter[ srcUpperStr[srcStrIdx+k0]] == 0 &&  srcUpperStr[srcStrIdx+k0] != '.')))
                        {
                            //  look for continuation, if:
                            //  k > 1  and  NO '-' in first string
                            n0 = -1;
                            
                            if (numMatchLetters > 1  &&  srcUpperStr[srcStrIdx+numMatchLetters] != '\0'  &&  p0 !=  '-') {
                                c0 = srcUpperStr[srcStrIdx+numMatchLetters-1];
                                n0 = alphaPos[ c0];
                                
                                if (n0 >= 2  &&  srcUpperStr[srcStrIdx+numMatchLetters] != '\0') {

                                    p_hash1 = Functions.Slice(phonetHash1, n0-2);
                                    p_hash2 = Functions.Slice(phonetHash2, n0-2);
                                    n0 = alphaPos[ srcUpperStr[srcStrIdx+numMatchLetters]];
                                    start3 = p_hash1 [n0];
                                    start4 = p_hash1 [0];
                                    end3 = p_hash2 [n0];
                                    end4 = p_hash2 [0];
                                    
                                    //  preserve rule priorities
                                    if (start4 >= 0 && (start3 < 0  ||  start4 < start3)) {
                                        n0 = start3;
                                        start3 = start4;
                                        start4 = n0;
                                        n0 = end3;
                                        end3 = end4;
                                        end4 = n0;
                                    }
                                    
                                    if (end3 >= start4  &&  start4 >= 0) {
                                        if (end4 > end3) {
                                            end3 = end4;
                                        }
                                        start4 = -1;
                                        end4 = -1;
                                    }
                                } else {
                                    n0 = phonetHash [ c0];
                                    start3 = n0;
                                    end3 = 10000;
                                    start4 = -1;
                                    end4 = -1;
                                }
                                
                                n0 = start3;
                            } // end if look for continuation
                            
                            if (n0 >= 0) {
                                // check continuation rules for "src[i+k]"
                                while (phonetRules[n0] == null ||  (phonetRules[n0].Length != 0 && phonetRules[n0][0] == c0)) {
                                    if (n0 > end3) {
                                        if (start4 > 0) {
                                            n0 = start4;
                                            start3 = start4;  start4 = -1;
                                            end3 = end4;  end4 = -1;
                                            continue;
                                        }
                                        p0 = -1;  // ****  important  ****
                                        break;
                                    }
                                    
                                    if (phonetRules [n0] == null ||  phonetRules [n0+mode] == null) { // XXX n0+1
                                        // no conversion rule available
                                        if (doTrace) {
                                            PhonetTracer.TraceInfo(phonetRules, "> > no rule found.", n0, "");
                                        }

                                        n0 += 3;
                                        continue;
                                    }
                                    if (doTrace) {
                                        PhonetTracer.TraceInfo(phonetRules, "> > continuation rule no.", n0, "is being checked");
                                    }
                                    
                                    // check whole string
                                    k0 = numMatchLetters;
                                    p0 = 5;
                                    nextRuleCharArr = phonetRules[n0] + "\0";
                                    nextRuleIdx = 1;
                                    while (nextRuleCharArr[nextRuleIdx] != '\0'  &&
                                           srcUpperStr[srcStrIdx+k0] == nextRuleCharArr[nextRuleIdx] &&
                                           !Char.IsDigit( nextRuleCharArr[nextRuleIdx]) &&
                                           "(-<^$".IndexOf(nextRuleCharArr[nextRuleIdx]) == -1) {
                                        k0++;
                                        nextRuleIdx++;
                                    }
                                    if (nextRuleCharArr[nextRuleIdx] == '(') {
                                        // check an array of letters
                                        if (isLetter[ srcUpperStr[srcStrIdx+k0]] != 0 &&
                                            Functions.strchr(nextRuleCharArr, nextRuleIdx+1, srcUpperStr[srcStrIdx+k0]) != -1) {
                                            k0++;
                                            while (nextRuleCharArr[nextRuleIdx] != '\0' &&
                                                   nextRuleCharArr[nextRuleIdx] != ')') {
                                                nextRuleIdx++;
                                            }
                                            if (nextRuleCharArr[nextRuleIdx] == ')') {
                                                nextRuleIdx++;
                                            }
                                        }
                                    }
                                    while (nextRuleCharArr[nextRuleIdx] == '-') {
                                        // "k0" is NOT decremented
                                        // because of  "if (k0 == k)"
                                        nextRuleIdx++;
                                    }
                                    if (nextRuleCharArr[nextRuleIdx] == '<') {
                                        nextRuleIdx++;
                                    }
                                    if (Char.IsDigit( nextRuleCharArr[nextRuleIdx])) {
                                        p0 = nextRuleCharArr[nextRuleIdx] - '0';
                                        nextRuleIdx++;
                                    }
                                    
                                    // *s == '^' is not possible here
                                    if ( nextRuleCharArr[nextRuleIdx] == '\0' ||
                                         (nextRuleCharArr[nextRuleIdx] == '$'  &&
                                         isLetter[ srcUpperStr[srcStrIdx+k0]] == 0 &&
                                         srcUpperStr[srcStrIdx+k0] != '.')) {
                                        
                                        if (k0 == numMatchLetters) {
                                            //  this is only a partial string
                                            if (doTrace) {
                                                PhonetTracer.TraceInfo(phonetRules, "> > continuation rule no.", n0, "not used (too short)");
                                            }
                                            n0 += 3;
                                            continue;
                                        }
                                        
                                        if (p0 < rulePriority) {
                                            //  priority is too low
                                            if (doTrace) {
                                                PhonetTracer.TraceInfo(phonetRules, "> > continuation rule no.", n0, "not used (priority)");
                                            }
                                            n0 += 3;
                                            continue;
                                        }
                                        
                                        // continuation rule found
                                        break;
                                    } // end if
                                    
                                    if (doTrace) {
                                        PhonetTracer.TraceInfo(phonetRules, "> > continuation rule no.", n0, "not used");
                                    }
                                    n0 += 3;
                                } // end of "while"
                                
                                if (p0 >= rulePriority &&
                                    (phonetRules[n0] != null  && phonetRules[n0].Length>0 && phonetRules[n0][0] == c0)) {
                                    
                                    if (doTrace) {
                                        PhonetTracer.TraceInfo(phonetRules, "> rule no.", n, "");
                                        PhonetTracer.TraceInfo(phonetRules, "> not used because of continuation", n0, "");
                                    }
                                    n += 3; // XXX: += 2
                                    continue;
                                }
                            } // end if n0 >= 0
                            
                            // replace string
                            if (doTrace) {
                                PhonetTracer.TraceInfo(phonetRules, "Rule no.", n, "is applied");
                            }
                            p0 = (phonetRules[n][0] != '\0' &&
                                  Functions.strchr (phonetRules[n], 1,'<') != -1) ?  1 : 0;
                            
                            nextRuleCharArr = phonetRules[n+mode] + "\0"; // XXX n+1
                            nextRuleIdx = 0;
                            
                            if (p0 == 1  &&  z == 0) {
                                
                                // rule with '<' is applied
                                if (doTrace) {
                                    Console.WriteLine("rule with < applied");
                                }

                                if (resultIdx > 0  &&
                                    nextRuleCharArr[nextRuleIdx] != '\0' &&
                                    (result[resultIdx-1] == nextCurrentChar  ||
                                    result[resultIdx-1] == nextRuleCharArr[nextRuleIdx])) {
                                    resultIdx--;
                                }
                                
                                z0 = 1;
                                z++;
                                k0 = 0;

                                while (nextRuleCharArr[nextRuleIdx] != '\0'  &&  srcUpperStr[srcStrIdx+k0] != '\0') {
                                    srcUpperStr[srcStrIdx+k0] = nextRuleCharArr[nextRuleIdx];
                                    k0++;
                                    nextRuleIdx++;
                                }

                                if (k0 < numMatchLetters) {

                                    int index = Functions.strchr(srcUpperStr, 0, '\0');
                                    Array.Copy(srcUpperStr, srcStrIdx+numMatchLetters, srcUpperStr, srcStrIdx+k0, index + 1 - srcStrIdx - numMatchLetters);

                                }

                                if (doCheckRules &&  (nextRuleCharArr[nextRuleIdx] != '\0'  ||  k0 > numMatchLetters)) {
                                    // we do "check_rules":
                                    // replacement string is too long
                                    result[resultIdx] = '\0';
                                    throw new Exception("Replacement String too long.");
                                }
                                //  new "current char"
                                nextCurrentChar = srcUpperStr[srcStrIdx];
                            } else {
                                if ((doCheckRules) &&  p0 == 1  &&  z > 0) {
                                    //  we do "check_rules":
                                    // recursion found -> error
                                    result[resultIdx] = '\0';
                                    throw new Exception("Recursion found");

                                }
                                
                                srcStrIdx = srcStrIdx+numMatchLetters-1;
                                z = 0;
                                while (nextRuleCharArr[nextRuleIdx] != '\0' &&  nextRuleCharArr[nextRuleIdx+1] != '\0'  &&  resultIdx < resultLen-1) {
                                    if (resultIdx == 0  ||  result[resultIdx-1] != nextRuleCharArr[nextRuleIdx]) {
                                        result[resultIdx] = nextRuleCharArr[nextRuleIdx];
                                        resultIdx++;
                                    }
                                    nextRuleIdx++;
                                }
                                
                                // new "current char"
                                nextCurrentChar = nextRuleCharArr[nextRuleIdx];
                                
                                 
                                if (phonetRules[n][0] != '\0' &&  phonetRules[n].IndexOf("^^", 1) != -1) {
                                    if (nextCurrentChar != '\0') {
                                        result[resultIdx] = nextCurrentChar;
                                        resultIdx++;
                                    }
                                    
                                    int index = Functions.strchr(srcUpperStr, 0, '\0');
                                    Array.Copy(srcUpperStr, srcStrIdx+1, srcUpperStr, 0, (index - (srcStrIdx + 1)) + 1);

                                    srcStrIdx = 0;
                                    z0 = 1;
                                }
                            }
                            
                            break;
                        }
                        
                        n += 3; // XXX: += 2
                        if (n > end1  &&  start2 > 0) {
                            n = start2;
                            start1 = start2;
                            end1 = end2;
                            start2 = -1;
                            end2 = -1;
                        }
                        
                    } // while (phonet_rules[n] == NULL  ||  phonet_rules[n][0] == c)
                    
                } // end if n >= 0
                
                if (z0 == 0) {
                    
                    if (resultIdx < resultLen-1  &&  nextCurrentChar != '\0' && (resultIdx == 0  ||  result[resultIdx-1] != nextCurrentChar)) {
                        
                        // delete multiple letters only
                        result[resultIdx] = nextCurrentChar;
                        resultIdx++;
                    }
                    srcStrIdx++;
                    z = 0;
                    
                } // end if z0 == 0
                
            } // end while iterate src
            
            result[resultIdx] = '\0';

            if (doTrace) {
                Console.WriteLine("<phonet() : " + new String(result));
                Console.WriteLine("====================================================");
            }
            
            String resultString = new String(result);
            int resIndex = resultString.IndexOf('\0');
            return resultString.Substring(0, resIndex);


        } // end phonetize
		
		
	}
}