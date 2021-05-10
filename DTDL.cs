
using Opc.Ua;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UANodesetWebViewer.Models;

namespace UANodesetWebViewer
{
    class DTDL
    {
        private static Dictionary<string, string> _map = new Dictionary<string, string>();

        private static void CreateSchemaMap()
        {
            // OPC UA built-in types: https://reference.opcfoundation.org/v104/Core/docs/Part6/5.1.2/
            // DTDL types: https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#primitive-schemas

            _map.Clear();
            _map.Add("1", "boolean");
            _map.Add("2", "integer");
            _map.Add("3", "integer");
            _map.Add("4", "integer");
            _map.Add("5", "integer");
            _map.Add("6", "integer");
            _map.Add("7", "integer");
            _map.Add("8", "long");
            _map.Add("9", "long");
            _map.Add("10", "float");
            _map.Add("11", "double");
            _map.Add("12", "string");
            _map.Add("13", "dateTime");
            _map.Add("19", "integer");
        }

        private static string GetDtdlDataType(string id)
        {
            try
            {
                return _map[id];
            }
            catch
            {
                return "string"; // default to string
            }
        }

        public static string GeneratedDTDL { get; set; }

        public static void AddNodeToDTDLInterface(NodeState uaNode, List<DtdlContents> interfaceContents)
        {
            CreateSchemaMap();

            // OPC UA defines variables, views and objects, as well as associated variabletypes, datatypes, referencetypes and objecttypes
            // In addition, OPC UA defines methods and properties

            BaseVariableState variableState = uaNode as BaseVariableState;
            if ((variableState != null) && (uaNode.DisplayName.ToString() != "InputArguments"))
            {
                DtdlContents dtdlTelemetry = new DtdlContents
                {
                    Type = "Telemetry",
                    Name = Regex.Replace(uaNode.DisplayName.ToString().Trim(), "[^A-Za-z]+", ""),
                    Schema = GetDtdlDataType(variableState.DataType.Identifier.ToString())
                };

                if (!interfaceContents.Contains(dtdlTelemetry))
                {
                    interfaceContents.Add(dtdlTelemetry);
                }
            }

            ViewState viewState = uaNode as ViewState;
            if (viewState != null)
            {
                // we don't map views since DTDL has no such concept
            }

            BaseObjectState objectState = uaNode as BaseObjectState;
            if (objectState != null)
            {
                // we don't map objects since DTDL has no such concept
            }

            BaseVariableTypeState variableTypeState = uaNode as BaseVariableTypeState;
            if (variableTypeState != null)
            {
                // we don't map UA custom types, only instances. DTDL only has a limited set of built-in types.
            }

            DataTypeState dataTypeState = uaNode as DataTypeState;
            if (dataTypeState != null)
            {
                // we don't map UA custom types, only instances. DTDL only has a limited set of built-in types.
            }

            ReferenceTypeState referenceTypeState = uaNode as ReferenceTypeState;
            if (referenceTypeState != null)
            {
                // we don't map UA custom types, only instances. DTDL only has a limited set of built-in types.
            }

            BaseObjectTypeState objectTypeState = uaNode as BaseObjectTypeState;
            if (objectTypeState != null)
            {
                // we don't map UA custom types, only instances. DTDL only has a limited set of built-in types.
            }

            MethodState methodState = uaNode as MethodState;
            if (methodState != null)
            {
                DtdlContents dtdlCommand = new DtdlContents
                {
                    Type = "Command",
                    Name = Regex.Replace(uaNode.DisplayName.ToString().Trim(), "[^A-Za-z]+", "")
                };

                if (!interfaceContents.Contains(dtdlCommand))
                {
                    interfaceContents.Add(dtdlCommand);
                }
            }

            PropertyState propertyState = uaNode as PropertyState;
            if (propertyState != null)
            {
                // we don't map UA node properties since DTDL has no such concept
            }
        }
    }
}
