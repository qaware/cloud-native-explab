variable "project_id" {
  description = "GCP project id"
  type        = string
  default     = "cloud-native-experience-lab"
}

variable "region" {
  description = "GCP region"
  type        = string
  default     = "europe-west1"
}

variable "gke_cluster_name" {
  default     = "tf-plain-cluster"
  description = "GKE cluster name"
  type        = string
}

variable "gke_num_nodes" {
  default     = 3
  description = "Number of GKE nodes"
  type        = "number"
}
