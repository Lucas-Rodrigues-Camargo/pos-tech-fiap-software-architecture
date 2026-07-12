variable "aws_region" {
  description = "Região AWS usada por todos os recursos."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Prefixo usado nos recursos AWS."
  type        = string
  default     = "oficina-mecanica"
}

variable "kubernetes_version" {
  description = "Versão do Kubernetes no EKS."
  type        = string
  default     = "1.33"
}

variable "node_instance_types" {
  description = "Tipos de instância dos workers do EKS."
  type        = list(string)
  default     = ["t3.small"]
}

variable "database_username" {
  description = "Usuário administrador do PostgreSQL."
  type        = string
  default     = "postgres"
}

variable "database_password" {
  description = "Senha do PostgreSQL."
  type        = string
  sensitive   = true
  validation {
    condition     = length(var.database_password) >= 12
    error_message = "database_password deve possuir pelo menos 12 caracteres."
  }
}
