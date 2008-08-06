/*
 * phonet.c
 * --------
 *
 * Program for phonetic string conversion.
 *
 * Copyright (c):
 * 1999-2003:  Joerg MICHAEL, Adalbert-Stifter-Str. 11, 30655 Hannover, Germany
 *     and
 * (version 1.0) 1999:  Heise Verlag, Helstorfer Str. 7, 30625 Hannover, Germany
 *
 * SCCS: @(#) phonet.c  1.3.4  2003-08-16
 *
 * This program is subject to the GNU Lesser General Public License (LGPL)
 * (formerly known as GNU Library General Public Licence)
 * as published by the Free Software Foundation; either version 2 of the
 * License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * You should have received a copy of the GNU Library General Public License
 * along with this program; if not, write to the
 * Free Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 * Actually, the LGPL is __less__ restrictive than the better known GNU General
 * Public License (GPL). See the GNU Library General Public License or the file
 * COPYING.LIB for more details and for a DISCLAIMER OF ALL WARRANTIES.
 *
 * There is one important restriction: If you modify this program in any way
 * (e.g. add or change phonetic rules or modify the underlying logic or
 * translate this program into another programming language), you must also
 * release the changes under the terms of the LGPL.
 * That means you have to give out the source code to your changes,
 * and a very good way to do so is mailing them to the address given below.
 * I think this is the best way to promote further development and use
 * of this software.
 *
 * If you have any remarks, feel free to e-mail to:
 *     ct@ct.heise.de
 */


#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

void *malloc (size_t);

#include "ph_ext.h"
#include "phonet.h"


#define  TEST_char    '\004'

/****  Macros for "phonet_init" and "run_mode":  ****/
#define  IS_INITIALIZED    1
#define  DO_TRACE          2
#define  CHECK_RULES       4


static int  run_mode = 0;
static int  last_rule_set = -SECOND_RULES;
static char upperchar[HASH_COUNT];
static int  alpha_pos[HASH_COUNT];
static int  isletter[HASH_COUNT];



static void trace_info (char text[], int n, char err_text[])
/****  output trace info  ****/
{
  char *s,*s2,*s3;
  s  = (phonet_rules[n] == NULL)  ?  (char *) "(NULL)" : phonet_rules[n];
  s2 = (phonet_rules[n+1] == NULL) ? (char *) "(NULL)" : phonet_rules[n+1];
  s3 = (phonet_rules[n+2] == NULL) ? (char *) "(NULL)" : phonet_rules[n+2];

  printf ("%s %d:  \"%s\"%s\"%s\" %s\n", text, ((n/3)+1), s,s2,s3, err_text);
}



static int initialize_phonet (void)

/****  language dependant initializations  ****/
/****  resut:  0 : success                 ****/
/****         -1 : an error occured        ****/
{
  int i,k,*p;
  int *p_hash1,*p_hash2;
  char *s,*s2;
  char temp[2];

  if (! (run_mode & IS_INITIALIZED))
    {
     if ((int)strlen (letters_a_to_z) > 26)
       {
        if (run_mode & DO_TRACE)
          {
           printf ("Error: %s  is not allowed\n",
                "strlen (letters_a_to_z) > 26");
          }
        return (-1);
       }
     if ((int)strlen (letters_a_to_z) != (int)strlen (letters_A_to_Z))
       {
        if (run_mode & DO_TRACE)
          {
           printf ("Error: %s  is not allowed\n",
                "strlen(letters_a_to_z) != strlen(letters_a_to_z)");
          }
        return (-1);
       }
     if ((int)strlen (umlaut_lower) != (int)strlen (umlaut_upper))
       {
        if (run_mode & DO_TRACE)
          {
           printf ("Error: %s  is not allowed\n",
                "strlen(umlaut_lower) != strlen(umlaut_upper)");
          }
        return (-1);
       }

     run_mode = run_mode | IS_INITIALIZED;

     /****  generate arrays "alpha_pos", "upperchar" and "isletter"  ****/
     for (i=0; i< HASH_COUNT; i++)
       {
        alpha_pos[i] = 0;
        isletter[i] = 0;
        upperchar[i] = (char) i;
       }

     for (k=-1; k<1; k++)
       {
        if (k == -1)
          {
           /****  German and international umlauts  ****/
           s = umlaut_lower;
           s2 = umlaut_upper;
           p = &k;
          }
        else
          {
           /****  "normal" letters ('a'-'z' and 'A'-'Z')  ****/
           s = letters_a_to_z;
           s2 = letters_A_to_Z;
           p = &i;
          }

        for (i=0; *(s+i) != '\0'; i++)
          {
           alpha_pos [(unsigned char) *(s+i)] = *p + 2;
           isletter [(unsigned char) *(s+i)] = 1;
           upperchar [(unsigned char) *(s+i)] = *(s2+i);

           alpha_pos [(unsigned char) *(s2+i)] = *p + 2;
           isletter [(unsigned char) *(s2+i)] = 1;
           upperchar [(unsigned char) *(s2+i)] = *(s2+i);
          }
       }
    }

  if (phonet_init != NULL  &&  phonet_hash != NULL  &&  phonet_rules != NULL)
    {
     *phonet_init = *phonet_init | IS_INITIALIZED;

     for (i=0; i< HASH_COUNT; i++)
       {
        phonet_hash[i] = -1;
       }

     for (i=0; i<26; i++)
       {
        p_hash1 = (* phonet_hash_1) [i];
        p_hash2 = (* phonet_hash_2) [i];

        for (k=0; k<28; k++)
          {
           p_hash1[k] = -1;
           p_hash2[k] = -1;
          }
       }

     for (i=0; phonet_rules[i] != PHONET_END; i += 3)
       {
        if ((s=phonet_rules[i]) != NULL)
          {
           /****  calculate first hash value  ****/
           k = (unsigned char) *s;

           if (phonet_hash[k] < 0
           && (phonet_rules[i+1] != NULL  ||  phonet_rules[i+2] != NULL))
             {
              phonet_hash[k] = i;
             }

           /****  calculate second hash values  ****/
           if (k != 0  &&  alpha_pos[k] >= 2)
             {
              k = alpha_pos[k];
              p_hash1 = (* phonet_hash_1) [k-2];
              p_hash2 = (* phonet_hash_2) [k-2];
              s++;

              if (*s == '(')
                {
                 s++;
                }
              else if (*s == '\0')
                {
                 s = (char *) " ";
                }
              else
                {
                 sprintf (temp, "%c", *s);
                 s = temp;
                }

              while (*s != '\0'  &&  (unsigned char) *s != ')')
                {
                 k = alpha_pos [(unsigned char) *s];

                 if (k > 0)
                   {
                    /****  add hash value for this letter  ****/
                    if (p_hash1[k] < 0)
                      {
                       p_hash1[k] = i;
                       p_hash2[k] = i;
                      }

                    if (p_hash2[k] >= i - 30)
                      {
                       p_hash2[k] = i;
                      }
                    else
                      {
                       k = -1;
                      }
                   }

                 if (k <= 0)
                   {
                    /****  add hash value for all letters  ****/
                    if (p_hash1[0] < 0)
                      {
                       p_hash1[0] = i;
                      }
                    p_hash2[0] = i;
                   }
                 s++;
                }
             }
          }
       }

     return (0);
    }

  return (-1);
}



#ifdef PHONET_EXECUTABLE

static void string_prepare (char *text, char *s, char *s2)
/****  Auxiliary function for "check_rules":  ****/
/****  "strcpy (text,s)" plus inclusion of    ****/
/****  'TEST_char' and '-', if necessary      ****/
{
  if (*s != '\0')
    {
     *text = *s;
     text++;
     s++;
    }
  while (*s != '\0'  &&  ! isdigit ((unsigned char) *s)
  &&  strchr ("-<^$", *s) == NULL)
    {
     *text = *s;
     text++;
     s++;
    }
  if (strchr (s2,'-') != NULL  ||  strchr (s2,'$') == NULL)
    {
     *text = TEST_char;
     text++;
     *text = '-';
     text++;
    }
  strcpy (text, s);
}

#endif




int phonet (char src[], char dest[], int len, int mode)

/****  Function for phonetic conversions          ****/
/****  ("dest" == "src" is allowed).              ****/
/****  "len" = max. length of "dest" incl. '\0'.  ****/
/****  mode = <language> + FIRST_RULES   :        ****/
/****           Use <language> and first rules    ****/
/****  mode = <language> + SECOND_RULES  :        ****/
/****           Use <language> and second rules   ****/
/****  result:  >= 0 :  string length of "dest"   ****/
/****           < 0  :  an error occured          ****/
{
 int  i,j,k,n,p,z;
 int  k0,n0,p0,z0;
 int  start1,end1,start2,end2;
 int  start3,end3,start4,end4;
 int  *p_hash1,*p_hash2;
 char c,c0,*s;
 char *src_2,text[51];

 if (dest == NULL  ||  src == NULL  ||  len <= 0)
   {
    /****  wrong arg's  ****/
    if (run_mode & DO_TRACE)
      {
       printf ("Error: wrong arguments.\n");
      }
    return (-1);
   }

 /****  select language  ****/
 i = 0;
 if (mode != last_rule_set)
   {
    i = set_phonet_language (mode % SECOND_RULES);
    last_rule_set = mode;
   }
 if (i < 0)
   {
    s = "Warning: language not found, use current language";
    if (phonet_init == NULL
    ||  phonet_hash == NULL  ||  phonet_rules == NULL)
      {
       i = set_phonet_language (PHONET_DEFAULT_LANGUAGE);
       s = "Warning: language not found, use default language";
       if (i < 0)
         {
          s = "Error: language not found; default language could not be set";
         }
      }
    if (run_mode & DO_TRACE)
      {
       printf ("%s\n", s);
      }
    if (phonet_init == NULL  ||  phonet_hash == NULL  ||  phonet_rules == NULL)
      {
       strcpy (dest,"");
       return (-2);
      }
   }

 if (phonet_init == NULL  ||  ! (*phonet_init & IS_INITIALIZED)
 ||  phonet_hash == NULL  ||  phonet_rules == NULL
 ||  ! (run_mode & IS_INITIALIZED))
   {
    /****  initialization  (must be done           ****/
    /****  BEFORE converting "src" to upper char)  ****/
    i = initialize_phonet();
    if (i < 0)
      {
       if (run_mode & DO_TRACE)
         {
          printf ("Error: initialization failed\n");
         }
       strcpy (dest,"");
       return (-3);
      }
   }

 src_2 = text;
 i = (int) strlen (src);
 if (i > 50)
   {
    /****  "oversized" string  ****/
    src_2 = malloc ((size_t) (i+1));
    if (src_2 == NULL)
      {
       /****  "malloc" failed  ****/
       if (run_mode & DO_TRACE)
         {
          printf ("Error: \"malloc\" for %d Bytes failed.\n", i+1);
         }
       strcpy (dest,"");
       return (-4);
      }
   }

 /****  "strcpy" plus conversion to upper char  ****/
 i = 0;
 while (src[i] != '\0')
   {
    src_2[i] = upperchar [(unsigned char) src[i]];
    i++;
   }
 src_2[i] = '\0';
 src = src_2;

 if (mode < SECOND_RULES)
   {
    mode = 1;
    s = "first";
   }
 else
   {
    mode = 2;
    s = "second";
   }
 if (run_mode & DO_TRACE)
   {
    printf ("\n\nphonetic conversion for  :  \"%s\"\n", src_2);
    printf ("(%s rules)\n", s);
   }

 /****  check "src"  ****/
 i = 0;
 j = 0;
 z = 0;
 while ((c = src[i]) != '\0')
   {
    if (run_mode & DO_TRACE)
      {
       printf ("\ncheck position %d:  src = \"%s\",", j, src+i);
       printf ("  dest = \"%.*s\"\n", j, dest);
      }

    n = alpha_pos [(unsigned char) c];
    if (n >= 2)
      {
       p_hash1 = (* phonet_hash_1) [n-2];
       p_hash2 = (* phonet_hash_2) [n-2];
       n = alpha_pos [(unsigned char) src[i+1]];
       start1 = p_hash1 [n];
       start2 = p_hash1 [0];
       end1 = p_hash2 [n];
       end2 = p_hash2 [0];

       /****  preserve rule priorities  ****/
       if (start2 >= 0
       && (start1 < 0  ||  start2 < start1))
         {
          n = start1;  start1 = start2;  start2 = n;
          n = end1;  end1 = end2;  end2 = n;
         }

       if (end1 >= start2  &&  start2 >= 0)
         {
          if (end2 > end1)
            {
             end1 = end2;
            }
          start2 = -1;
          end2 = -1;
         }
      }
    else
      {
       n = phonet_hash [(unsigned char) c];
       start1 = n;
       end1 = 10000;
       start2 = -1;
       end2 = -1;
      }

    n = start1;
    z0 = 0;

    if (n >= 0)
      {
       /****  check rules for this char  ****/
       while (phonet_rules[n] == NULL  ||  phonet_rules[n][0] == c)
         {
          if (n > end1)
            {
             if (start2 > 0)
               {
                n = start2;
                start1 = start2;  start2 = -1;
                end1 = end2;  end2 = -1;
                continue;
               }
             break;
            }

          if (phonet_rules [n] == NULL  ||  phonet_rules [n+mode] == NULL)
            {
             /****  no conversion rule available  ****/
             n += 3;
             continue;
            }
          if (run_mode & DO_TRACE)
            {
             trace_info ("> rule no.", n, "is being checked");
            }

          /****  check whole string  ****/
          k = 1;   /****  no. of matching letters  ****/
          p = 5;   /****  default priority  ****/
          s = phonet_rules[n];
          s++;     /****  needed by "*(s-1)" below  ****/

          while (*s != '\0'  &&  src[i+k] == *s
          &&  ! isdigit ((unsigned char) *s)
          &&  strchr ("(-<^$", *s) == NULL)
            {
             k++;
             s++;
            }
          if (run_mode & CHECK_RULES)
            {
             /****  we do "check_rules"  ****/
             while (*s != '\0'  &&  src[i+k] == *s)
               {
                k++;
                s++;
               }
            }
          if (*s == '(')
            {
             /****  check an array of letters  ****/
             if (isletter [(unsigned char) src[i+k]]
             &&  strchr (s+1, src[i+k]) != NULL)
               {
                k++;
                while (*s != '\0'  &&  *s != ')')
                  {
                   s++;
                  }
                if (*s == ')')
                  {
                   s++;
                  }
               }
            }
          p0 = (int) *s;
          k0 = k;
          while (*s == '-'  &&  k > 1)
            {
             k--;
             s++;
            }
          if (*s == '<')
            {
             s++;
            }
          if (isdigit ((unsigned char) *s))
            {
             /****  read priority  ****/
             p = *s - '0';
             s++;
            }
          if (*s == '^'  &&  *(s+1) == '^')
            {
             s++;
             if ((run_mode & CHECK_RULES)
             &&  ! isletter [(unsigned char) src[i+k0]])
               {
                /****  we do "check_rules"  ****/
                s = s-2;
               }
            }

          if (*s == '\0'
          || (*s == '^'  &&  (i == 0  ||  ! isletter [(unsigned char)src[i-1]])
           && (*(s+1) != '$'
           || (! isletter [(unsigned char) src[i+k0]]  &&  src[i+k0] != '.')))
          || (*s == '$'  &&  i > 0  &&  isletter [(unsigned char) src[i-1]]
           && (! isletter [(unsigned char) src[i+k0]]  &&  src[i+k0] != '.')))
            {
             /****  look for continuation, if:         ****/
             /****  k > 1  and  NO '-' in first string ****/
             n0 = -1;

             if (k > 1  &&  src[i+k] != '\0'  &&  p0 != (int) '-')
               {
                c0 = src [i+k-1];
                n0 = alpha_pos [(unsigned char) c0];

                if (n0 >= 2  &&  src[i+k] != '\0')
                  {
                   p_hash1 = (* phonet_hash_1) [n0-2];
                   p_hash2 = (* phonet_hash_2) [n0-2];
                   n0 = alpha_pos [(unsigned char) src[i+k]];
                   start3 = p_hash1 [n0];
                   start4 = p_hash1 [0];
                   end3 = p_hash2 [n0];
                   end4 = p_hash2 [0];

                   /****  preserve rule priorities  ****/
                   if (start4 >= 0
                   && (start3 < 0  ||  start4 < start3))
                     {
                      n0 = start3;  start3 = start4;  start4 = n0;
                      n0 = end3;  end3 = end4;  end4 = n0;
                     }

                   if (end3 >= start4  &&  start4 >= 0)
                     {
                      if (end4 > end3)
                        {
                         end3 = end4;
                        }
                      start4 = -1;
                      end4 = -1;
                     }
                  }
                else
                  {
                   n0 = phonet_hash [(unsigned char) c0];
                   start3 = n0;
                   end3 = 10000;
                   start4 = -1;
                   end4 = -1;
                  }

                n0 = start3;
               }

             if (n0 >= 0)
               {
                /****  check continuation rules for "src[i+k]"  ****/
                while (phonet_rules[n0] == NULL
                ||  phonet_rules[n0][0] == c0)
                  {
                   if (n0 > end3)
                     {
                      if (start4 > 0)
                        {
                         n0 = start4;
                         start3 = start4;  start4 = -1;
                         end3 = end4;  end4 = -1;
                         continue;
                        }
                      p0 = -1;  /****  important  ****/
                      break;
                     }

                   if (phonet_rules [n0] == NULL
                   ||  phonet_rules [n0+mode] == NULL)
                     {
                      /****  no conversion rule available  ****/
                      n0 += 3;
                      continue;
                     }
                   if (run_mode & DO_TRACE)
                     {
                      trace_info ("> > continuation rule no.",
                          n0, "is being checked");
                     }

                   /****  check whole string  ****/
                   k0 = k;
                   p0 = 5;
                   s = phonet_rules[n0];
                   s++;
                   while (*s != '\0'  &&  src[i+k0] == *s
                   &&  ! isdigit ((unsigned char) *s)
                   &&  strchr("(-<^$",*s) == NULL)
                     {
                      k0++;
                      s++;
                     }
                   if (*s == '(')
                     {
                      /****  check an array of letters  ****/
                      if (isletter [(unsigned char) src[i+k0]]
                      &&  strchr (s+1, src[i+k0]) != NULL)
                        {
                         k0++;
                         while (*s != '\0'  &&  *s != ')')
                           {
                            s++;
                           }
                         if (*s == ')')
                           {
                            s++;
                           }
                        }
                     }
                   while (*s == '-')
                     {
                      /****  "k0" is NOT decremented     ****/
                      /****  because of  "if (k0 == k)"  ****/
                      s++;
                     }
                   if (*s == '<')
                     {
                      s++;
                     }
                   if (isdigit ((unsigned char) *s))
                     {
                      p0 = *s - '0';
                      s++;
                     }

                   if (*s == '\0'
                     /****  *s == '^' is not possible here  ****/
                   || (*s == '$'  &&  ! isletter [(unsigned char) src[i+k0]]
                    &&  src[i+k0] != '.'))
                     {
                      if (k0 == k)
                        {
                         /****  this is only a partial string  ****/
                         if (run_mode & DO_TRACE)
                           {
                            trace_info ("> > continuation rule no.",
                                n0, "not used (too short)");
                           }
                         n0 += 3;
                         continue;
                        }

                      if (p0 < p)
                        {
                         /****  priority is too low  ****/
                         if (run_mode & DO_TRACE)
                           {
                            trace_info ("> > continuation rule no.",
                                n0, "not used (priority)");
                           }
                         n0 += 3;
                         continue;
                        }

                      /****  continuation rule found  ****/
                      break;
                     }

                   if (run_mode & DO_TRACE)
                     {
                      trace_info ("> > continuation rule no.",
                          n0, "not used");
                     }
                   n0 += 3;
                  } /****  end of "while"  ****/

                if (p0 >= p
                && (phonet_rules[n0] != NULL  &&  phonet_rules[n0][0] == c0))
                  {
                   if (run_mode & DO_TRACE)
                     {
                      trace_info ("> rule no.", n,"");
                      trace_info ("> not used because of continuation",n0,"");
                     }
                   n += 3;
                   continue;
                  }
               }

             /****  replace string  ****/
             if (run_mode & DO_TRACE)
               {
                trace_info ("Rule no.", n, "is applied");
               }
             p0 = (phonet_rules[n][0] != '\0'
                &&  strchr (phonet_rules[n]+1,'<') != NULL) ?  1 : 0;
             s = phonet_rules [n+mode];

             if (p0 == 1  &&  z == 0)
               {
                /****  rule with '<' is applied  ****/
                if (j > 0  &&  *s != '\0'
                && (dest[j-1] == c  ||  dest[j-1] == *s))
                  {
                   j--;
                  }
                z0 = 1;
                z++;
                k0 = 0;
                while (*s != '\0'  &&  src[i+k0] != '\0')
                  {
                   src[i+k0] = *s;
                   k0++;
                   s++;
                  }
                if (k0 < k)
                  {
                   strcpy (src+i+k0, src+i+k);
                  }
                if ((run_mode & CHECK_RULES)
                &&  (*s != '\0'  ||  k0 > k))
                  {
                   /****  we do "check_rules":            ****/
                   /****  replacement string is too long  ****/
                   dest[j] = '\0';
                   return (-200);
                  }
                /****  new "current char"  ****/
                c = src[i];
               }
             else
               {
                if ((run_mode & CHECK_RULES)
                &&  p0 == 1  &&  z > 0)
                  {
                   /****  we do "check_rules":      ****/
                   /****  recursion found -> error  ****/
                   dest[j] = '\0';
                   return (-100);
                  }
                i = i+k-1;
                z = 0;
                while (*s != '\0'
                &&  *(s+1) != '\0'  &&  j < len-1)
                  {
                   if (j == 0  ||  dest[j-1] != *s)
                     {
                      dest[j] = *s;
                      j++;
                     }
                   s++;
                  }
                /****  new "current char"  ****/
                c = *s;

                if (phonet_rules[n][0] != '\0'
                &&  strstr (phonet_rules[n]+1, "^^") != NULL)
                  {
                   if (c != '\0')
                     {
                      dest[j] = c;
                      j++;
                     }
                   src += i+1;
                   i = 0;
                   z0 = 1;
                  }
               }

             break;
            }

          n += 3;
          if (n > end1  &&  start2 > 0)
            {
             n = start2;
             start1 = start2;
             end1 = end2;
             start2 = -1;
             end2 = -1;
            }
         }
      }

    if (z0 == 0)
      {
       if (j < len-1  &&  c != '\0'
       && (j == 0  ||  dest[j-1] != c))
         {
          /****  delete multiple letters only  ****/
          dest[j] = c;
          j++;
         }
       i++;
       z = 0;
      }
   }

 if (src_2 != text)
   {
    free (src_2);
   }
 dest[j] = '\0';
 return (j);
}




#ifdef PHONET_EXECUTABLE

int check_rules (int language, int trace_only)
/****  Check all phonetic rules of the current    ****/
/****  language.                                  ****/
/****  ("trace_only" > 0:  trace this rule only)  ****/
/****  Result:  Number of errors                  ****/
{
 int  i,k,n,n0;
 int  errors = 0;
 int  rule_count = 0;
 char *r,*r0,rule[35];
 char *s,err_text[201];
 char orig[35],orig2[35];
 char text[35],text2[35];

 /****  initialization  ****/
 i = set_phonet_language (language);
 if (i >= 0)
   {
    i = initialize_phonet();
   }
 if (i < 0)
   {
    printf ("Error: initialization for language %d failed\n", language);
    return (-1);
   }

 isletter [(unsigned char) TEST_char] = 1;
 run_mode = run_mode | CHECK_RULES;
 i = 0;

 while (phonet_rules[i] != PHONET_END)
   {
    /****  syntax check for all strings  ****/
    if ((i/3)+1 == trace_only)
      {
       run_mode = run_mode | DO_TRACE;
      }
    else if (trace_only > 0)
      {
       run_mode = run_mode & (~DO_TRACE);
      }

    strcpy (err_text,"");
    k = 0;
    if (i % 3 == 0)
      {
       if (phonet_rules[i] == NULL
       || (phonet_rules[i+1] == NULL  &&  phonet_rules[i+2] == NULL))
         {
          strcpy (err_text,"  Forbidden null pointer");
          k = -10;
         }
       rule_count++;
      }

    if (k >= 0)
      {
       if (phonet_rules[i] == NULL)
         {
          i++;
          continue;
         }

       if (i % 3 == 0)
         {
          /****  check first letter  ****/
          s = phonet_rules[i];
          n = phonet_hash [(unsigned char) *s];
          if (i >= n+3  &&  n >= 0
          &&  (s == NULL  ||  *s != phonet_rules[i-3][0]))
            {
             strcpy (err_text,"  Wrong first char");
             k = -10;
            }

          if (k >= 0)
            {
             /****  check length of search string  ****/
             k = 0;
             while (*s != '\0'  &&  ! isdigit ((unsigned char) *s)
             &&  strchr ("()<^$", *s) == NULL)
               {
                k++;
                s++;
               }
             if (k == 0)
               {
                strcpy (err_text,"  Search string is empty");
                if (*s != '\0'  &&  strchr ("()<^$", *s) == NULL)
                  {
                   strcpy (err_text,"  First char is meta char");
                  }
                k = -10;
               }
            }
         }
      }

    if (k >= 0)
      {
       /****  syntax check for string  ****/
       k = 0;
       s = phonet_rules[i];
       n = 0;
       if (*s != upperchar [(unsigned char) *s])
         {
          /****  forbidden lower-case char  ****/
          k = -100;
         }
       if (i % 3 == 0  &&  *s != '\0')
         {
          s++;
          n++;
         }
       while (*s != '\0'  &&  k >= 0)
         {
          if (*s != upperchar [(unsigned char) *s])
            {
             /****  forbidden lower-case char  ****/
             k = -100;
             break;
            }
          if (*s == '(')
            {
             if (k >= 1  ||  ! isletter [(unsigned char) *(s+1)])
               {
                k = -10;
                break;
               }
             s++;
             n++;
             while (isletter[(unsigned char) *s])
               {
                s++;
               }
             if (*s != ')')
               {
                k = -10;
                break;
               }
             k = 1;
            }
          else if (*s == '-')
            {
             /****  "k > 2" is correct              ****/
             /****  (more than one '-' is allowed)  ****/
             n--;
             if (k > 2  ||  n <= 0)
               {
                k = -10;
                break;
               }
             k = 2;
            }
          else if (*s == '<')
            {
             if (k >= 3)
               {
                k = -10;
                break;
               }
             k = 3;
            }
          else if (isdigit ((unsigned char) *s))
            {
             if (k >= 4)
               {
                k = -10;
                break;
               }
             k = 4;
            }
          else if (*s == '^')
            {
             if (k >= 5)
               {
                k = -10;
                break;
               }
             if (*(s+1) == '^')
               {
                s++;
               }
             k = 5;
            }
          else if (*s == '$')
            {
             if (k >= 6  ||  *(s+1) != '\0')
               {
                k = -10;
                break;
               }
             k = 6;
            }
          else if (k > 0  ||  *s == ')')
            {
             k = -10;
             break;
            }
          else
            {
             n++;
            }
          s++;
         }

       if (k > 0  &&  i % 3 != 0)
         {
          sprintf (err_text,"  Meta char in replacement string");
          k = -10;
         }
       else if (k <= -100)
         {
          sprintf (err_text,"  Lower-case letter in string");
         }
       else if (k < 0)
         {
          sprintf (err_text,"  Syntax error in search string");
         }
       else if ((int) strlen (phonet_rules[i]) > 30)
         {
          sprintf (err_text,"  String very long ( > 30 chars)");
          k = -1;
         }
       s = phonet_rules[i];

       if (k >= 0  &&  i % 3 == 0
       &&  n > 0  &&  strchr (s,'<') != NULL)
         {
          /****  check lengths of search and replacement string  ****/
          if ((phonet_rules[i+1] != NULL
           &&  strcmp (s,phonet_rules[i+1]) == 0)
          || (phonet_rules[i+2] != NULL
           &&  strcmp (s,phonet_rules[i+2]) == 0))
             {
              strcpy (err_text,"  Replacement string too long due to '<'");
              k = -10;
             }
          if ((phonet_rules[i+1] != NULL
           && (int) strlen (phonet_rules[i+1]) > n)
          || (phonet_rules[i+2] != NULL
           && (int) strlen (phonet_rules[i+2]) > n))
             {
              strcpy (err_text,"  Replacement string too long due to '<'");
              k = -10;
             }
         }
      }

    if (k < 0)
      {
       /****  output error message  ****/
       s = "Possible error in rule";
       if (k < -1)
         {
          s = "Error in rule";
         }
       trace_info (s, i-(i%3), err_text);
       errors++;
      }


    if (k >= 0  &&  i % 3 != 0)
      {
       /****  do phonetic conversion and check result  ****/
       n = i % 3;
       n0 = (i%3 == 1) ?  FIRST_RULES : SECOND_RULES;
       r = strchr (phonet_rules[i-n], '(');
       if (r == NULL)
         {
          /****  There is no regular expression in search string  ****/
          r = "  ";
         }
       r++;

       while (*r != ')'  &&  *r != '\0')
         {
          /****  Split regular expression (e.g. "GS(CH)--")  ****/
          /****  into simple rules and check each of them.   ****/
          r0 = phonet_rules[i-n];
          strcpy (rule, r0);
          phonet_rules[i-n] = rule;
          s = strchr (rule,'(');

          if (s != NULL)
            {
             *s = *r;
             s++;
             while (*s != ')'  &&  *s != '\0')
               {
                strcpy (s,s+1);
               }
             if (*s == ')')
               {
                strcpy (s,s+1);
               }
            }

          /****  do the check  ****/
          sprintf (orig, "%c%s", TEST_char, phonet_rules[i-n]);
          sprintf (orig2, "%c%s", TEST_char, phonet_rules[i]);

          if (strchr (phonet_rules[i-n],'^') != NULL)
            {
             sprintf (orig, orig+1);
             sprintf (orig2,orig2+1);
            }
          if (strchr (phonet_rules[i-n],'-') != NULL
          ||  strchr (phonet_rules[i-n],'$') == NULL)
            {
             sprintf (orig, "%s%c", orig, TEST_char);
             sprintf (orig2,"%s%c", orig2,TEST_char);
            }
          if (orig2[0] == orig2[1]  &&  orig2[2] == '\0')
            {
             /****  e.g. orig2 == "<TEST_char><TEST_char>"  ****/
             orig2[1] = '\0';
            }

          /****  check conversion result  ****/
          k = phonet (orig,text, 33,n0);
          if (k > -100)
            {
             k = phonet (orig2,text2, 33,n0);
            }

          if (k <= -100)
            {
             /****  error found  ****/
             phonet_rules[i-n] = r0;
             strcpy (err_text,"  Recursion found");
             if (k == -200)
               {
                strcpy (err_text,"  Replacement string too long due to '<'");
               }
             trace_info ("Error in rule", i-(i%3), err_text);
             errors++;
             break;
            }

          /****  second rule check  ****/
          if (strcmp (text,orig2) != 0)
            {
             string_prepare (err_text+80, rule,rule);
             string_prepare (err_text, orig,orig);

             phonet_rules[i-n] = err_text+80;
             (void) phonet (err_text, err_text+40, 33,n0);
             phonet_rules[i-n] = rule;
             err_text[0] = '\0';
             if (strcmp (err_text+40, orig2) == 0)
               {
                strcpy (text,orig2);
               }
            }

          if (strcmp (text2,orig2) != 0
          && ((strcmp (phonet_rules[i-n],"AVIER$") == 0  &&  n==1
           &&  strcmp (phonet_rules[i],"AWIE") == 0)
          || (strcmp (phonet_rules[i-n],"GH") == 0  &&  n == 1
           &&  strcmp (phonet_rules[i],"G") == 0)
          || (strcmp (phonet_rules[i-n],"HEAD-") == 0  &&  n == 1
           &&  strcmp (phonet_rules[i],"HE") == 0)
          || (strcmp (phonet_rules[i-n],"IERRE$") == 0
           &&  strcmp (phonet_rules[i],"IER") == 0)
          || (strcmp (phonet_rules[i-n],"IVIER$") == 0  &&  n == 1
           &&  strcmp (phonet_rules[i],"IWIE") == 0)
          || (strcmp (phonet_rules[i-n],"SHST") == 0  &&  n == 1
           &&  strcmp (phonet_rules[i],"SHT") == 0)))
             {
              /****  these are exceptions  ****/
              strcpy (text2, orig2);
             }

     #ifdef PHONET_GERMAN
          if (strcmp (text2,orig2) != 0
          &&  language == PHONET_GERMAN
          && ((strncmp (phonet_rules[i-n],"GEGEN",5) == 0  &&  n == 1
           &&  strncmp (phonet_rules[i],"GEGN",4) == 0)
          || (strcmp (phonet_rules[i-n],"GGF.") == 0  &&  n == 1
           &&  strcmp (phonet_rules[i],"GF.") == 0)
          || (strcmp (phonet_rules[i-n],"HAVEN7$") == 0  &&  n == 1
           &&  strcmp (phonet_rules[i],"HAFN") == 0)
          || (strcmp (phonet_rules[i-n],"IEDENSTELLE------") == 0
           &&  n == 1  &&  strcmp (phonet_rules[i],"IDN ") == 0)
          || (strcmp (phonet_rules[i-n],"INDELERREGE------") == 0
           &&  n == 1  &&  strcmp (phonet_rules[i],"INDL ") == 0)))
             {
              /****  exceptions in German  ****/
              strcpy (text2, orig2);
             }
     #endif

          if (strcmp (text2,orig2) != 0
          && (s = strchr (orig2,'I')) != NULL)
            {
             /****  extra check for replacement strings with an 'I'  ****/
             if (strchr (s+1,'I') != NULL)
               {
                /****  take second 'I', if found  ****/
                s = strchr (s+1,'I');
               }
             *s = 'J';
             (void) phonet (orig2,text2, 33,n0);
             *s = 'I';
            }

          /****  extra check for search strings with a '-'  ****/
          s = orig;
          k = 0;
          while (*s != '\0'  &&  ! isdigit ((unsigned char) *s)
          &&  strchr ("-<^$",*s) == NULL)
            {
             s++;
             k++;
            }
          while (*s != '\0')
            {
             if (*s == '-')
               {
                k--;
               }
             s++;
            }

          if (strcmp (text2,orig2) != 0
           && ((strchr (orig,'-') != NULL  &&  k > 0)
          || (phonet_rules[i-n][0] == phonet_rules[i-n][1]
           &&  phonet_rules[i-n][0] == phonet_rules[i][0])
          || (strncmp (phonet_rules[i-n],"AI",2) == 0
           &&  phonet_rules[i][0] == 'E'
           &&  k > 1  &&  strncmp (s-2,"E$",2) == 0)))
            {
             s = orig + k;
             k = (int) strlen (orig2);
             if (k > 0)
               {
                if (orig2[k-1] == TEST_char)
                  {
                   k--;
                  }
                strcpy (err_text+1, orig2);
                strcpy (err_text+1+k, s);
                k = 1;

                if (phonet_rules[i-n][0] == phonet_rules[i-n][1]
                &&  phonet_rules[i-n][0] == phonet_rules[i][0]
                &&  phonet_rules[i][1] == '\0')
                  {
                   /****  extra check for double letters  ****/
                   err_text[0] = TEST_char;
                   err_text[1] = phonet_rules[i][0];
                   k = 0;
                  }
                if (phonet_rules[i-n][0] == 'H'
                &&  phonet_rules[i-n][1] != '\0'
                &&  phonet_rules[i-n][2] == 'H'
                &&  phonet_rules[i-n][1] == phonet_rules[i][0]
                &&  phonet_rules[i-n][2] == phonet_rules[i][1])
                  {
                   /****  special case "H?H"  ****/
                   err_text[0] = TEST_char;
                   err_text[1] = 'H';
                   k = 0;
                  }
                if (strncmp (phonet_rules[i-n],"LV",2) == 0
                &&  strncmp (phonet_rules[i], "LW",2) == 0)
                  {
                   /****  special case "LV*"  ****/
                   err_text[3] = 'V';
                  }
                if (strncmp (phonet_rules[i-n],"AI",2) == 0
                &&  phonet_rules[i][0] == 'E')
                  {
                   /****  special case "AI*E$"  ****/
                   err_text[0] = TEST_char;
                   err_text[1] = err_text[2];
                   strcpy (err_text+2, phonet_rules[i]);
                   k = 0;
                  }

                (void) phonet (err_text+k, err_text+40, 33,n0);

                if (strcmp (err_text+40, orig2) != 0)
                  {
                   string_prepare (err_text+80, err_text+k,rule);
                   string_prepare (err_text, rule,rule);

                   phonet_rules[i-n] = err_text;
                   (void) phonet (err_text+80, err_text+40, 33,n0);
                   phonet_rules[i-n] = rule;
                  }
                err_text[0] = '\0';
                if (strcmp (err_text+40, orig2) == 0)
                  {
                   strcpy (text2, orig2);
                  }
               }
            }

          phonet_rules[i-n] = r0;

          if (strcmp (text, orig2) != 0
          ||  strcmp (text2,orig2) != 0)
            {
             orig[0] = '\0';
             if (*r != ' ')
               {
                sprintf (orig," for '%c'", *r);
               }
             sprintf (err_text, "  result %d%s: \"%s\"%s\"",
                 n,orig, text,text2);

             /****  delete 'TEST_char' from "error" string  ****/
             s = err_text;
             while (*s != '\0')
               {
                while (*s == TEST_char)
                  {
                   strcpy (s,s+1);
                  }
                s++;
               }

             /****  output error message  ****/
             s = "Possible error in rule";
             if (strcmp (text,orig2) != 0)
               {
                s = "Error in rule";
               }
             trace_info (s, i-(i%3), err_text);
             errors++;
            }
          r++;
         }
      }
    i++;
   }

 if (i % 3 != 0)
   {
    printf ("Error: string count is not a multiple of 3.\n");
    errors++;
   }
 isletter [(unsigned char) TEST_char] = 0;
 run_mode = run_mode & (~CHECK_RULES);

 printf ("Language \"%s\"  (%d phonetic rules):\n", phonet_language, rule_count);
 printf ("Check of all phonetic rules:  ");

 if (errors == 0)
   {
    printf ("No syntax error or inconsistency found.\n");
   }
 else
   {
    printf ("%d errors have been found.\n\n", errors);
    printf ("Remarks:\n");
    printf ("a) The correct syntax for search strings is:\n");
    printf ("      <word> [<->..] [<] [<0-9>] [^[^]] [$]\n");
    printf ("   The end of <word> may contain as a simple regular expression\n");
    printf ("   one array of letters that must be enclosed in '(' and ')'.\n");
    printf ("b) Rules with a '<' demand that the replacement string may not\n");
    printf ("   be longer than the search string.\n");
    printf ("c) The placement of rules determines their priority.\n");
    printf ("   Therefore, the rules for \"SH\" must be placed before the rules\n");
    printf ("   for \"S\" (otherwise, a conversion error will occur for \"SH\").\n");
    printf ("d) Another common source of errors is ignorance of dependencies.\n");
    printf ("   For example, in German the replacement string \"NJE\" would be wrong,\n");
    printf ("   because the 'J' is subject to another phonetic rule.\n");
   }

 return (errors);
}



int main (int argc, char *argv[])
{
  FILE *fr;
  char *s,text[201];
  int  n=0,i=-1,r=-1;

  if (argc < 2
  ||  strcmp (argv[1], "-?") == 0
  ||  strcmp (argv[1], "-h") == 0
  ||  strcmp (argv[1], "-help") == 0)
    {
     printf ("Usage:  phonet  <orig>       [ <language> ]  [ -trace ]\n");
     printf (" or  :  phonet -file  <file>  <FIRST_RULES | SECOND_RULES>  [ <language> ]\n");
     printf (" or  :  phonet -check_rules  [ <language> ]  [ -trace [<rule_no>] ]\n");
     printf (" or  :  phonet -write_rules  [ <language> ]\n");
     printf ("\n");
     printf ("Program for phonetic string conversion  (%s).\n\n", PHONET_VERSION);
     printf ("Options:\n");
     printf ("-file <file> :  Phonetically convert the given file.\n");
     printf ("-check_rules :  Check all phonetic rules. If no language is\n");
     printf ("                specified, all rules of all languages are checked.\n");
     printf ("\n");
     printf ("-trace       :  Output trace info. If a rule number is specified\n");
     printf ("                for \"-check_rules\", then only this rule will be\n");
     printf ("                traced.\n\n");
     printf ("Language may be one of the following numbers:\n");

     for (i=FIRST_RULES; i< SECOND_RULES; i++)
       {
        if (set_phonet_language(i) >= 0)
          {
           s = "";
           if (i == PHONET_DEFAULT_LANGUAGE)
             {
              s = "  (default language)";
             }
           printf (" %2d:  %s%s\n", i,phonet_language,s);
          }
       }
     return (1);
    }


   /****  parse arguments  ****/
  if (argc >= 4  &&  strcmp (argv[1], "-file") == 0)
    {
     if (strncmp (argv[3], "FIRST",5) == 0
     ||  strncmp (argv[3], "first",5) == 0)
       {
        r = FIRST_RULES;
       }
     else if (strncmp (argv[3], "SECOND",6) == 0
     ||  strncmp (argv[3], "second",6) == 0)
       {
        r = SECOND_RULES;
       }
     else
       {
        printf ("Warning:  rule set not specified; using first rules\n");
        r = FIRST_RULES;
       }

     i = PHONET_DEFAULT_LANGUAGE;
     if (argc >= 5)
       {
        i = atoi (argv[4]);
       }
     if (i < 0  ||  set_phonet_language(i) < 0)
       {
        i = PHONET_DEFAULT_LANGUAGE;
       }
     (void) set_phonet_language (i);

     /****  convert file  ****/
     if ((fr = fopen (argv[2],"r")) == NULL)
       {
        printf ("Error:  could not open source file '%s'\n", argv[2]);
        return (1);
       }

     while (! feof (fr))
       {
        /****  read data  ****/
        if (fgets (text,200,fr) != NULL)
          {
           i = (int) strlen (text);
           if (i > 0  &&  text[i-1] == '\n')
             {
              /****  important  ****/
              text[i-1] = '\0';
              if (i == 1)
                {
                 continue;
                }
             }

           phonet (text, text,201, r);
           printf ("%s\n", text);
          }
       }

     fclose (fr);
     return (0);
    }

  if (argc >= 3  &&  isdigit ((unsigned char) argv[2][0]))
    {
     i = atoi (argv[2]);
     if (argc >= 4  &&  strcmp (argv[3], "-trace") == 0)
       {
        if (argc >= 5  &&  atoi (argv[4]) > 0)
          {
           r = atoi (argv[4]);
          }
        run_mode = run_mode | DO_TRACE;
       }
    }
  if (argc >= 3  &&  strcmp (argv[2], "-trace") == 0)
    {
     if (argc >= 4  &&  atoi (argv[3]) > 0)
       {
        r = atoi (argv[3]);
       }
     run_mode = run_mode | DO_TRACE;
    }

   /****  check_rules  ****/
  if (strcmp (argv[1], "-check_rules") == 0)
    {
     if (i >= 0)
       {
        n = check_rules (i,r);
       }
     else
       {
        for (i=FIRST_RULES; i< SECOND_RULES; i++)
          {
           if (set_phonet_language(i) >= 0)
             {
              n += check_rules (i,r);
              printf ("\n\n");
             }
          }
       }
     return (n);
    }

  /****  phonet conversion of string "argv[1]"  ****/
  if (i < 0  ||  set_phonet_language(i) < 0)
    {
     i = PHONET_DEFAULT_LANGUAGE;
    }
  (void) set_phonet_language (i);

  strcpy (text,"            ");
  s = argv[1];
  if ((int) strlen (s) > 200)
    {
     strcpy (text, "(too long; shortened)");
     s[200] = '\0';
    }
  printf ("Original string %s:  \"%s\"\n", text, s);
  printf ("(language = %s)\n\n", phonet_language);

  phonet (s, text,201, FIRST_RULES);
  printf ("Conversion with first  rules:  \"%s\"\n", text);

  phonet (s, text,201, SECOND_RULES);
  printf ("Conversion with second rules:  \"%s\"\n", text);

  return (0);
}

#endif
