using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostree
{
    class Ghostree
    {
        private enum OpCode
        {
            STACK___dummy__ = 0,
            STACK_dup,
            STACK_swap,
            STACK_rotate,
            STACK_pop,
            CALC_add = 100,
            CALC_sub,
            CALC_mul,
            CALC_div,
            CALC_mod,
            OUTPUT_num_out = 200,
            OUTPUT_char_out,
            INPUT_num_in = 300,
            INPUT_char_in,
            exit = 10000,
            clear,
            copy,
            push,
            label,
            jump,
        }

        private enum OpCodeType
        {
            STACK = 0,
            CALC = 100,
            OUTPUT = 200,
            INPUT = 300,
        }

        private static int OP_STACK_SIZE = 5;
        private static int OP_CALC_SIZE = 5;
        private static int OP_OUTPUT_SIZE = 2;
        private static int OP_INPUT_SIZE = 2;

        class Operation
        {
            public OpCode code { get; set; }
            public int param { get; set; }

            public Operation(OpCode c) {
                code = c;
            }
            public Operation(OpCode c, int p) {
                code = c;
                param = p;
            }
        }
        public static void Run(byte[] src) {
            new Ghostree(src).Run();
        }

        public static void DebugRun(byte[] src) {
            var g = new Ghostree(src);
            foreach (var operation in g.insns) {
                Console.WriteLine($"{operation.code.ToString()}[{operation.param}]");
            }
            g.Run();
        }
        private bool comment { get; set; }
        private List<Operation> insns { get; set; }
        private Stack<int> stack { get; set; }
        private Dictionary<int, int> labels { get; set; }

        private Ghostree(byte[] src) {
            comment = false;
            insns = Parse(src);
            stack = new Stack<int>();
            labels = FindLabels(insns);
        }

        private List<Operation> Parse(byte[] src) {
            var opList = new List<Operation>();
            var spaces = 0;
            foreach (var b in src) {
                var c = Convert.ToChar(b);
                if (c == '>') {
                    comment = false;
                }
                else if (c == '<') {
                    comment = true;
                }

                if (!comment) {
                    if (c == ' ') {
                        spaces += 1;
                    }
                    else if ("/|\\'+*^`".Contains(c) == false) {
                        //skip this char
                    }
                    else {
                        switch (c) {
                            case '*': {
                                    opList.Add(new Operation(OpCode.exit));
                                    break;
                                }
                            case '`': {
                                    if (spaces == 0) {
                                        opList.Add(new Operation(OpCode.clear));
                                    }
                                    else {
                                        opList.Add(new Operation(OpCode.copy, spaces));
                                    }
                                    break;
                                }
                            case '/': {
                                    if (spaces == 0) {
                                        throw new Exception("lacking spaces before '/'");
                                    }

                                    if (spaces < OP_STACK_SIZE) {
                                        opList.Add(GetOperation(OpCodeType.STACK, spaces));
                                    }
                                    else {
                                        opList.Add(new Operation(OpCode.push, spaces - OP_STACK_SIZE));
                                    }
                                    break;
                                }
                            case '\\': {
                                    opList.Add(GetOperation(OpCodeType.CALC, spaces));
                                    break;
                                }
                            case '|': {
                                    opList.Add(GetOperation(OpCodeType.OUTPUT, spaces));
                                    break;
                                }
                            case '\'': {
                                    opList.Add(GetOperation(OpCodeType.INPUT, spaces));
                                    break;
                                }
                            case '+': {
                                    opList.Add(new Operation(OpCode.label, spaces));
                                    break;
                                }
                            case '^': {
                                    opList.Add(new Operation(OpCode.jump, spaces));
                                    break;
                                }
                        }

                        spaces = 0;
                    }
                }
            }

            return opList;
        }

        private Operation GetOperation(OpCodeType type, int n) {
            var size = 1;
            switch (type) {
                case OpCodeType.STACK: size = OP_STACK_SIZE; break;
                case OpCodeType.CALC: size = OP_CALC_SIZE; break;
                case OpCodeType.OUTPUT: size = OP_OUTPUT_SIZE; break;
                case OpCodeType.INPUT: size = OP_INPUT_SIZE; break;
            }

            return new Operation((OpCode)(n % size + type));
        }

        private Dictionary<int, int> FindLabels(List<Operation> opList) {
            var labels = new Dictionary<int, int>();
            for (var i = 0; i < opList.Count; i++) {
                var o = opList[i];
                var insn = o.code;
                var arg = o.param;
                if (insn == OpCode.label) {
                    if (labels.ContainsKey(arg)) {
                        throw new Exception($"duplicate label {arg}");
                    }
                    labels.Add(arg, i);
                }
            }

            return labels;
        }

        private void Run() {
            var pc = 0;
            while (pc < insns.Count) {
                var insn = insns[pc].code;
                var arg = insns[pc].param;
                switch (insn) {
                    #region stack instructions
                    case OpCode.push: {
                            stack.Push(arg);
                            break;
                        }
                    case OpCode.STACK_dup: {
                            var x = stack.Pop();
                            stack.Push(x);
                            stack.Push(x);
                            break;
                        }
                    case OpCode.STACK_swap: {
                            var x = stack.Pop();
                            var y = stack.Pop();
                            stack.Push(x);
                            stack.Push(y);
                            break;
                        }
                    case OpCode.STACK_rotate: {
                            var z = stack.Pop();
                            var y = stack.Pop();
                            var x = stack.Pop();
                            stack.Push(z);
                            stack.Push(x);
                            stack.Push(y);
                            break;
                        }
                    case OpCode.STACK_pop: {
                            stack.Pop();
                            break;
                        }
                    case OpCode.copy: {
                            //和ruby版兼容
                            var index = stack.Count - arg;
                            index = index % stack.Count;
                            if (index < 0) {
                                index = index + stack.Count;
                            }
                            stack.Push(stack.ToArray()[index]);
                            break;
                        }
                    case OpCode.clear: {
                            stack.Clear();
                            break;
                        }
                    #endregion
                    #region arithmetic instructions
                    case OpCode.CALC_add: {
                            var y = stack.Pop();
                            var x = stack.Pop();
                            stack.Push(x + y);
                            break;
                        }
                    case OpCode.CALC_sub: {
                            var y = stack.Pop();
                            var x = stack.Pop();
                            stack.Push(x - y);
                            break;
                        }
                    case OpCode.CALC_mul: {
                            var y = stack.Pop();
                            var x = stack.Pop();
                            stack.Push(x * y);
                            break;
                        }
                    case OpCode.CALC_div: {
                            var y = stack.Pop();
                            var x = stack.Pop();
                            stack.Push(x / y);
                            break;
                        }
                    case OpCode.CALC_mod: {
                            var y = stack.Pop();
                            var x = stack.Pop();
                            stack.Push(x % y);
                            break;
                        }
                    #endregion
                    #region I/O instructions
                    case OpCode.OUTPUT_num_out: {
                            Console.Write(stack.Peek());
                            break;
                        }
                    case OpCode.OUTPUT_char_out: {
                            Console.Write(Convert.ToChar(stack.Peek()));
                            break;
                        }
                    case OpCode.INPUT_char_in: {
                            stack.Push(Console.ReadLine()?.FirstOrDefault() ?? 0);
                            break;
                        }
                    case OpCode.INPUT_num_in: {
                            var text = Console.ReadLine();
                            var num = 0;
                            Int32.TryParse(text.Trim(), out num);
                            stack.Push(num);
                            break;
                        }
                    #endregion
                    #region control flow instructions
                    case OpCode.label: {
                            //label's info is already process, so do nothing here
                            break;
                        }
                    case OpCode.jump: {
                            if (stack.Any() && stack.Peek() != 0) {
                                if (labels.ContainsKey(arg) == false) {
                                    throw new Exception($"jump target {arg} not found");
                                }
                                pc = labels[arg];
                            }
                            break;
                        }
                    case OpCode.exit: {
                            return;
                        }
                    default: {
                            throw new Exception($"[BUG] unknown instruction {insn}");
                        }
                        #endregion
                }

                pc++;
            }
        }

    }
}
