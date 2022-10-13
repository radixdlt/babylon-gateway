using System;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public enum InstructionOp
{
    NotFound,
    CallMethod,
    CallFunction,
    LockFee,
    FreeXrd,
    TakeFromWorktop,
    TakeFromWorktopByAmount,
    CreateNewAccount,
    WithdrawByAmount,
    Deposit,
}

public record InstructionParameter(string Name, string Value);

public record Person(string FirstName, string LastName, string Id)
{
    internal string Id { get; init; } = Id;
}

public record Instruction(InstructionOp Opcode, string Address, List<InstructionParameter> Parameters)
{
    public InstructionOp OpCode { get; } = Opcode;

    public string Address { get; set; } = Address;

    public List<InstructionParameter> Parameters { get; } = Parameters;
}

public static class ManifestParser
{
    private static readonly Dictionary<string, InstructionOp> OpCodesMap = new()
    {
        { "lock_fee", InstructionOp.LockFee },
        { "free_xrd", InstructionOp.FreeXrd },
        { "new_with_resource", InstructionOp.CreateNewAccount },
        { "withdraw_by_amount", InstructionOp.WithdrawByAmount },
        { "deposit", InstructionOp.Deposit },
        { "TAKE_FROM_WORKTOP", InstructionOp.TakeFromWorktop },
        { "TAKE_FROM_WORKTOP_BY_AMOUNT", InstructionOp.TakeFromWorktopByAmount },
    };

    public static List<Instruction> Parse(string manifest)
    {
        var manifestCalls = manifest.Trim().Split("\n");

        var instructions = new List<Instruction>();

        foreach (var manifestCall in manifestCalls)
        {
            var cmdParts = manifestCall.Trim().Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty)
                .Replace("\"", string.Empty)
                .Replace("\\", string.Empty).Replace(";", string.Empty).Split(" ");

            var opCode = InstructionOp.NotFound;
            var address = string.Empty;

            var parameters = new List<InstructionParameter>();

            foreach (var cmdPart in cmdParts)
            {
                if (string.IsNullOrWhiteSpace(cmdPart))
                {
                    continue;
                }

                if (OpCodesMap.Keys.Contains(cmdPart))
                {
                    opCode = OpCodesMap[cmdPart];
                }
                else if (cmdPart.Contains("ComponentAddress"))
                {
                    AddAddressOrParameter(cmdPart, "ComponentAddress", ref address, parameters);
                }
                else if (cmdPart.Contains("PackageAddress"))
                {
                    AddAddressOrParameter(cmdPart, "PackageAddress", ref address, parameters);
                }
                else if (cmdPart.Contains("ResourceAddress"))
                {
                    AddAddressOrParameter(cmdPart, "ResourceAddress", ref address, parameters);
                }
                else if (cmdPart.Contains("NonFungibleAddress"))
                {
                    AddParameter(cmdPart, "NonFungibleAddress", parameters);
                }
                else if (cmdPart.Contains("Decimal"))
                {
                    AddParameter(cmdPart, "Decimal", parameters);
                }
                else if (cmdPart.Contains("Bucket"))
                {
                    AddParameter(cmdPart, "Bucket", parameters);
                }
            }

            if (opCode == InstructionOp.NotFound || address == string.Empty)
            {
                throw new ArgumentException($"Cannot parse manifest call: {manifestCall}");
            }

            instructions.Add(new Instruction(opCode, address, parameters));
        }

        return instructions;
    }

    private static void AddParameter(string cmdPart, string pattern, List<InstructionParameter> parameters)
    {
        if (cmdPart.Contains(pattern))
        {
            parameters.Add(new InstructionParameter(pattern, cmdPart.Replace(pattern, string.Empty)));
        }
    }

    private static void AddAddressOrParameter(string cmdPart, string pattern, ref string address, List<InstructionParameter> parameters)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            address = cmdPart.Replace(pattern, string.Empty);
        }
        else
        {
            AddParameter(cmdPart, pattern, parameters);
        }
    }
}
